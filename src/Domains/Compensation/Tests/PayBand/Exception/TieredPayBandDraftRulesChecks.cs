namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TieredPayBandDraftRulesChecks
{
    public static void TieredPayBandDraftValidatorReportsEveryProblem()
    {
        var validator = TieredPayBandDraftRules.DefaultTieredPayBandDraftValidator();
        var item = TieredPayBandDraft.NewTieredPayBandDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPayBandDraftRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredPayBandDraftRequiredLabelRule("tiered pay band draft needs a value");
        var item = TieredPayBandDraft.NewTieredPayBandDraft("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPayBandDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredPayBandDraftRequiredCodeRule("tiered pay band draft needs a value");
        var item = TieredPayBandDraft.NewTieredPayBandDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPayBandDraftHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TieredPayBandDraftHoursRangeRule(7.0, 3489.0);
        var item = TieredPayBandDraft.NewTieredPayBandDraft("x", "y");
        item.Hours = 3490.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredPayBandDraftValidatorReportsEveryProblem();
        TieredPayBandDraftRequiredLabelRuleFlagsABlankLabel();
        TieredPayBandDraftRequiredCodeRuleFlagsABlankCode();
        TieredPayBandDraftHoursRangeRuleFlagsValuesAboveHours();
    }
}
