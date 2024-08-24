﻿using System.Collections.Generic;
using System.Linq;
using GameFrameX.CodeGenerator.Utils;
using GameFrameX.Setting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;

namespace GameFrameX.CodeGenerator.Agent;

[Generator]
public class AgentGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        ResLoader.LoadDll();
        context.RegisterForSyntaxNotifications(() => new AgentFilter());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is AgentFilter receiver)
        {
            receiver.ClearPartialList();
            //生成模板
            var str = ResLoader.LoadTemplate("Agent.liquid");
            var agentTemplate = Template.Parse(str);

            var partialClassCount = new Dictionary<string, int>();

            foreach (var agent in receiver.AgentList)
            {
                var fullName = agent.GetFullName();
                var info = new AgentInfo();
                info.Super = agent.Identifier.Text;
                info.Name = info.Super + GlobalConst.WrapperNameSuffix;
                //info.Space = Tools.GetNameSpace(fullName);
                info.Space = GlobalConst.HotfixNameSpaceNamePrefix + GlobalConst.WrapperNameSuffix + ".Agent";

                string outFileName = null;

                var isPartialClass = agent.Modifiers.ToList().FindIndex(s => s.Text == "partial") >= 0;
                if (isPartialClass)
                {
                    info.Partial = "partial";
                    partialClassCount.TryGetValue(info.Name, out var count);
                    partialClassCount[info.Name] = count + 1;
                    outFileName = $"{info.Name}{count}.g.cs";
                }
                else
                {
                    outFileName = $"{info.Name}.g.cs";
                }

                //处理Using
                var root = agent.SyntaxTree.GetCompilationUnitRoot();
                foreach (var element in root.Usings)
                {
                    info.Usingspaces.Add(element.Name.ToString());
                }

                info.Usingspaces.Add(Tools.GetNameSpace(fullName));

                foreach (var member in agent.Members)
                {
                    if (member is MethodDeclarationSyntax method)
                    {
                        if (method.Identifier.Text.Equals("Active")
                            || method.Identifier.Text.Equals("Inactive"))
                        {
                            continue;
                        }

                        var mth = new MthInfo();
                        //修饰符
                        foreach (var m in method.Modifiers)
                        {
                            if (m.Text.Equals("virtual"))
                            {
                                mth.IsVirtual = true;
                                mth.Modify += "override ";
                            }
                            else
                            {
                                mth.Modify += m.Text + " ";
                            }

                            if (m.Text.Equals("public"))
                            {
                                mth.IsPublic = true;
                            }

                            if (m.Text.Equals("static"))
                            {
                                mth.IsStatic = true;
                            }

                            if (m.Text.Equals("async"))
                            {
                                mth.Isasync = true;
                            }
                        }

                        if (mth.IsStatic)
                        {
                            continue;
                        }

                        mth.ReturnType = method.ReturnType?.ToString() ?? "void"; //Task<T>
                        //遍历注解
                        foreach (var a in method.AttributeLists)
                        {
                            var attStr = a.ToString().RemoveWhitespace();
                            if (attStr.Contains("[Api]") || attStr.Contains("[Service]"))
                            {
                                mth.IsApi = true;
                            }
                            else if (attStr.Contains("[Discard]"))
                            {
                                mth.Discard = true;
                                if (mth.Isasync)
                                {
                                    mth.Modify = mth.Modify.Replace("async ", "");
                                    mth.Isasync = false;
                                }
                            }
                            else if (attStr.Contains("TimeOut"))
                            {
                                mth.HasTimeout = true;
                                var argStr = a.Attributes[0].ArgumentList.Arguments[0].ToString();
                                if (argStr.Contains("timeout"))
                                {
                                    mth.Timeout = int.Parse(argStr.Split(':')[1]);
                                }
                                else
                                {
                                    mth.Timeout = int.Parse(a.Attributes[0].ArgumentList.Arguments[0].ToString());
                                }
                            }
                            else if (attStr.Contains("[ThreadSafe]"))
                            {
                                mth.Threadsafe = true;
                            }
                        }

                        if (mth.Threadsafe && mth.HasTimeout)
                        {
                            context.LogError($"{fullName}.{method.Identifier.Text}无法为标记【Threadsafe】的函数指定超时时间");
                        }

                        if (!mth.IsApi && !mth.Discard && mth.HasTimeout)
                        {
                            context.LogError($"{fullName}.{method.Identifier.Text}【Timeout】注解只能配合【Api】或【Discard】使用");
                        }

                        //跳过没有标记任何注解的函数
                        if (!mth.IsApi && !mth.Discard && !mth.Threadsafe)
                        {
                            continue;
                        }

                        //线程安全且没有丢弃直接跳过
                        if (mth.Threadsafe && !mth.Discard)
                        {
                            continue;
                        }

                        if (mth.IsApi && !mth.Threadsafe && !mth.ReturnType.Contains("Task"))
                        {
                            context.LogError($"{fullName}.{method.Identifier.Text}, 非【Threadsafe】的【Api】接口只能是异步函数");
                        }

                        if ((mth.IsApi || mth.Discard || mth.Threadsafe) && !mth.IsVirtual)
                        {
                            context.LogError($"{fullName}.{method.Identifier.Text}标记了【AsyncApi】【Threadsafe】【Discard】注解的函数必须申明为virtual");
                        }

                        if (mth.IsVirtual)
                        {
                            info.Methods.Add(mth);
                            mth.Name = method.Identifier.Text;
                            mth.ParamDeclare = method.ParameterList.ToString(); //(int a, List<int> list)
                            //mth.Returntype = method.ReturnType.ToString();   //Task<T>
                            if (mth.Discard && !mth.ReturnType.Equals("Task") && !mth.ReturnType.Equals("ValueTask"))
                            {
                                context.LogError($"{fullName}.{method.Identifier.Text}只有返回值为Task类型或ValueTask类型才能添加【Discard】注解");
                            }

                            mth.Constraint = method.ConstraintClauses.ToString(); //where T : class, new() where K : BagState
                            mth.Typeparams = method.TypeParameterList?.ToString(); //"<T, K>"	
                            foreach (var p in method.ParameterList.Parameters)
                            {
                                mth.Params.Add(p.Identifier.Text);
                            }
                        }
                    }
                }

                var source = agentTemplate.Render(info);
                context.AddSource(outFileName, source);
            }
        }
    }
}