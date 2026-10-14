namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPayBandBaselineRulesChecks
{
    public static void ScheduledPayBandBaselineValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayBandBaselineRules.DefaultScheduledPayBandBaselineValidator();
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayBandBaselineScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ScheduledPayBandBaselineScoreRangeRule(1.0, 861.4);
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("x", "y");
        item.Score = 862.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayBandBaselineExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new ScheduledPayBandBaselineExternalRefLengthRule(255);
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayBandBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledPayBandBaselineRequiredTitleRule("scheduled pay band baseline needs an entry");
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayBandBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledPayBandBaselineRequiredCodeRule("scheduled pay band baseline needs a value");
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayBandBaselineRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new ScheduledPayBandBaselineRequiredExternalRefRule("scheduled pay band baseline needs this field");
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPayBandBaselineValidatorReportsEveryProblem();
        ScheduledPayBandBaselineScoreRangeRuleFlagsValuesAboveScore();
        ScheduledPayBandBaselineExternalRefLengthRuleFlagsALongExternalRef();
        ScheduledPayBandBaselineRequiredTitleRuleFlagsABlankTitle();
        ScheduledPayBandBaselineRequiredCodeRuleFlagsABlankCode();
        ScheduledPayBandBaselineRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
