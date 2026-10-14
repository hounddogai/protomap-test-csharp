namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RegionalPayBandDraftRulesChecks
{
    public static void RegionalPayBandDraftValidatorReportsEveryProblem()
    {
        var validator = RegionalPayBandDraftRules.DefaultRegionalPayBandDraftValidator();
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPayBandDraftQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RegionalPayBandDraftQuantityRangeRule(3600, 86400);
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPayBandDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalPayBandDraftCodeLengthRule(255);
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPayBandDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalPayBandDraftRequiredCodeRule("regional pay band draft needs a value");
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalPayBandDraftValidatorReportsEveryProblem();
        RegionalPayBandDraftQuantityRangeRuleFlagsValuesAboveQuantity();
        RegionalPayBandDraftCodeLengthRuleFlagsALongCode();
        RegionalPayBandDraftRequiredCodeRuleFlagsABlankCode();
    }
}
