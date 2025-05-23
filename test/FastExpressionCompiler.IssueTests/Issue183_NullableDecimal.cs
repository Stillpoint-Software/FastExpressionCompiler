using System;


#pragma warning disable IDE1006 // Naming Styles for linq2db
#pragma warning disable 649 // Unassigned fields

#if LIGHT_EXPRESSION
using static FastExpressionCompiler.LightExpression.Expression;
namespace FastExpressionCompiler.LightExpression.IssueTests
#else
using static System.Linq.Expressions.Expression;
namespace FastExpressionCompiler.IssueTests
#endif
{
    public class Issue183_NullableDecimal : ITest
    {
        public int Run()
        {
            ConvertNullNullableParamToNullableDecimal_CheckAgainstTheSystemExprCompile();

            ConvertDecimalParamToNullableDecimal();
            ConvertDecimalPropertyToNullableDecimal();
            ConvertNullableBytePropertyToNullableDecimal();
            NullableDecimalIssue();
            return 5;
        }


        public void ConvertDecimalParamToNullableDecimal()
        {
            var param = Parameter(typeof(decimal), "d");

            var f = Lambda<Func<decimal, decimal?>>(Convert(param, typeof(decimal?)), param).CompileFast(true);
            var x = f(42);

            Asserts.IsNotNull(x);
            Asserts.AreEqual(42, x.Value);
        }


        public void ConvertNullNullableParamToNullableDecimal_CheckAgainstTheSystemExprCompile()
        {
            var ps = Parameter(typeof(byte?), "b");
            var e = Lambda<Func<byte?, decimal?>>(Convert(ps, typeof(decimal?)), ps);
            e.PrintCSharp();

            var fs = e.CompileSys();
            fs.PrintIL();
            var xs = fs(null);
            Asserts.IsNull(xs);

            var ff = e.CompileFast(true);
            ff.PrintIL();
            var xf = ff(null);
            Asserts.IsNull(xf);
        }


        public void ConvertDecimalPropertyToNullableDecimal()
        {
            var param = Parameter(typeof(DecimalContainer), "d");

            var f = Lambda<Func<DecimalContainer, decimal?>>(
                Convert(Property(param, nameof(DecimalContainer.Decimal)), typeof(decimal?)),
                param
                ).CompileFast(true);

            var x = f(new DecimalContainer { Decimal = 42 });

            Asserts.IsNotNull(x);
            Asserts.AreEqual(42, x.Value);
        }


        public void ConvertNullableBytePropertyToNullableDecimal()
        {
            var param = Parameter(typeof(DecimalContainer), "d");

            var f = Lambda<Func<DecimalContainer, decimal?>>(
                Convert(Property(param, nameof(DecimalContainer.NullableByte)), typeof(decimal?)),
                param
            ).CompileFast(true);

            var x = f(new DecimalContainer { NullableByte = 42 });

            Asserts.IsNotNull(x);
            Asserts.AreEqual(42, x.Value);
        }


        public void NullableDecimalIssue()
        {
            var param = Parameter(typeof(DecimalContainer));

            var body = Equal(
                Convert(Property(param, nameof(DecimalContainer.NullableByte)), typeof(decimal?)),
                Convert(Property(param, nameof(DecimalContainer.Decimal)), typeof(decimal?)));

            var f = Lambda<Func<DecimalContainer, bool>>(body, param).CompileFast(true);

            var x = f(new DecimalContainer { Decimal = 1 });
            Asserts.IsFalse(x); // cause byte? to decimal? would be `null`
        }
    }

    public class DecimalContainer
    {
        public byte? NullableByte { get; set; }
        public decimal Decimal { get; set; }
    }
}
