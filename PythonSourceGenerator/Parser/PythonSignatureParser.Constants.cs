﻿using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace PythonSourceGenerator.Parser;
public static partial class PythonSignatureParser
{
    public static TextParser<Unit> IntegerConstantToken { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from digits in Character.Digit.AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> DecimalConstantToken { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from digits in Character.Digit.Many().OptionalOrDefault(['0'])
        from decimal_ in Character.EqualTo('.')
        from rest in Character.Digit.Many()
        select Unit.Value;

    public static TextParser<Unit> HexidecimalConstantToken { get; } =
        from prefix in Span.EqualTo("0x")
        from digits in Character.HexDigit.AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> DoubleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('"')
        from chars in Character.ExceptIn('"').Many()
        from close in Character.EqualTo('"')
        select Unit.Value;

    public static TextParser<Unit> SingleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('\'')
        from chars in Character.ExceptIn('\'').Many()
        from close in Character.EqualTo('\'')
        select Unit.Value;

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> DoubleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.DoubleQuotedString)
        .Apply(ConstantParsers.DoubleQuotedString)
        .Select(s => new PythonConstant { IsString = true, StringValue = s })
        .Named("Double Quoted String Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> SingleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.SingleQuotedString)
        .Apply(ConstantParsers.SingleQuotedString)
        .Select(s => new PythonConstant { IsString = true, StringValue = s })
        .Named("Single Quoted String Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> DecimalConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Decimal)
        .Apply(Numerics.DecimalDouble)
        .Select(d => new PythonConstant { IsFloat = true, FloatValue = d })
        .Named("Decimal Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> IntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Integer)
        .Apply(Numerics.IntegerInt64)
        .Select(d => new PythonConstant { IsInteger = true, IntegerValue = d })
        .Named("Integer Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> HexidecimalIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.HexidecimalInteger)
        .Apply(ConstantParsers.HexidecimalConstantParser)
        .Select(d => new PythonConstant { IsInteger = true, IntegerValue = (long)d })
        .Named("Hexidecimal Integer Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> BoolConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.True).Or(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.False))
        .Select(d => new PythonConstant { IsBool = true, BoolValue = d.Kind == PythonSignatureTokens.PythonSignatureToken.True })
        .Named("Bool Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> NoneConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.None)
        .Select(d => new PythonConstant { IsNone = true })
        .Named("None Constant");

    // Any constant value
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant?> ConstantValueTokenizer { get; } =
        DecimalConstantTokenizer.AsNullable()
        .Or(IntegerConstantTokenizer.AsNullable())
        .Or(HexidecimalIntegerConstantTokenizer.AsNullable())
        .Or(BoolConstantTokenizer.AsNullable())
        .Or(NoneConstantTokenizer.AsNullable())
        .Or(DoubleQuotedStringConstantTokenizer.AsNullable())
        .Or(SingleQuotedStringConstantTokenizer.AsNullable())
        .Named("Constant");

    static class ConstantParsers
    {
        public static TextParser<string> DoubleQuotedString { get; } =
            from open in Character.EqualTo('"')
            from chars in Character.ExceptIn('"', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('"'))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('"')
            select new string(chars);

        public static TextParser<string> SingleQuotedString { get; } =
            from open in Character.EqualTo('\'')
            from chars in Character.ExceptIn('\'', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('\''))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('\'')
            select new string(chars);

        public static TextParser<UInt64> HexidecimalConstantParser { get; } =
            from prefix in Span.EqualTo("0x")
            from digits in Character.HexDigit.AtLeastOnce()
            select UInt64.Parse(new string(digits), System.Globalization.NumberStyles.HexNumber);
    }
}
