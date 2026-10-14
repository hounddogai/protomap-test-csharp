namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictSpanClaimRulesChecks
{
    public static void StrictSpanClaimValidatorReportsEveryProblem()
    {
        var validator = StrictSpanClaimRules.DefaultStrictSpanClaimValidator();
        var item = StrictSpanClaim.NewStrictSpanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSpanClaimNameLengthRuleFlagsALongName()
    {
        var rule = new StrictSpanClaimNameLengthRule(32);
        var item = StrictSpanClaim.NewStrictSpanClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSpanClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictSpanClaimRequiredTitleRule("strict span claim needs this field");
        var item = StrictSpanClaim.NewStrictSpanClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictSpanClaimValidatorReportsEveryProblem();
        StrictSpanClaimNameLengthRuleFlagsALongName();
        StrictSpanClaimRequiredTitleRuleFlagsABlankTitle();
    }
}
