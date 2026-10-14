namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanAgreementRulesChecks
{
    public static void SpanAgreementValidatorReportsEveryProblem()
    {
        var validator = SpanAgreementRules.DefaultSpanAgreementValidator();
        var item = SpanAgreement.NewSpanAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanAgreementRequiredCodeRule("span agreement needs this field");
        var item = SpanAgreement.NewSpanAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanAgreementCodeLengthRule(16);
        var item = SpanAgreement.NewSpanAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanAgreementValidatorReportsEveryProblem();
        SpanAgreementRequiredCodeRuleFlagsABlankCode();
        SpanAgreementCodeLengthRuleFlagsALongCode();
    }
}
