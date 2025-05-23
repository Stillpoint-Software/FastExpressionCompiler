﻿using System;
using System.Reflection;


#if LIGHT_EXPRESSION
using static FastExpressionCompiler.LightExpression.Expression;
namespace FastExpressionCompiler.LightExpression.IssueTests
#else
using static System.Linq.Expressions.Expression;
namespace FastExpressionCompiler.IssueTests
#endif
{

    public class Issue215_Assign_to_member_test : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }


        public void Test1()
        {
            var type = Type.GetType("System.Reflection.RtFieldInfo", false);
            var fieldInfo_m_Attributes = type?.GetField("m_fieldAttributes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldInfoParam = Parameter(typeof(FieldInfo));
            var castedType = Convert(fieldInfoParam, type);

            var getAttrExpr = Lambda<Func<FieldInfo, int>>(
                Convert(MakeMemberAccess(castedType, fieldInfo_m_Attributes), typeof(int)),
                fieldInfoParam);

            getAttrExpr.PrintCSharp();
            var getAttr = getAttrExpr.CompileFast(true);

            var returnLabel = Label();
            var nr = Lambda(
                Block(
                    Assign(MakeMemberAccess(castedType, fieldInfo_m_Attributes),
                        Convert(And(Convert(MakeMemberAccess(castedType, fieldInfo_m_Attributes), typeof(int)), Constant((int)(~FieldAttributes.InitOnly)))
                            , typeof(System.Reflection.FieldAttributes))
                        )
                    , Return(returnLabel)
                    , Label(returnLabel))
                , fieldInfoParam);

            nr.PrintCSharp();
            var nra = (Action<FieldInfo>)nr.CompileFast(true);
            Asserts.IsNotNull(nra);

            var fieldF = typeof(A).GetField(nameof(A.F));
            nra(fieldF);
            var x = getAttr(fieldF);
            Asserts.AreEqual(0, x & (int)FieldAttributes.InitOnly);

            var r = Lambda(
                Block(
                    Assign(MakeMemberAccess(castedType, fieldInfo_m_Attributes),
                        Convert(Or(Convert(MakeMemberAccess(castedType, fieldInfo_m_Attributes), typeof(int)), Constant((int)(FieldAttributes.InitOnly)))
                            , typeof(FieldAttributes))
                        )
                    , Return(returnLabel)
                    , Label(returnLabel))
                , fieldInfoParam);

            r.PrintCSharp();
            var ra = (Action<FieldInfo>)r.CompileFast(true);
            Asserts.IsNotNull(ra);
            ra(fieldF);
            var y = getAttr(fieldF);
            Asserts.AreNotEqual(0, y & (int)FieldAttributes.InitOnly);
        }

        public class A
        {
            public int F;
        }
    }
}
