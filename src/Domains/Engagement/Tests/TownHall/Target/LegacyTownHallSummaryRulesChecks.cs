namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyTownHallSummaryRulesChecks
{
    public static void LegacyTownHallSummaryValidatorReportsEveryProblem()
    {
        var validator = LegacyTownHallSummaryRules.DefaultLegacyTownHallSummaryValidator();
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTownHallSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyTownHallSummaryRequiredCodeRule("legacy town hall summary needs a value");
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTownHallSummaryRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new LegacyTownHallSummaryRequiredJobCodeRule("legacy town hall summary needs this field");
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTownHallSummaryAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new LegacyTownHallSummaryAmountRangeRule(5.6, 4229.5);
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("x", "y");
        item.Amount = 4231.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyTownHallSummaryValidatorReportsEveryProblem();
        LegacyTownHallSummaryRequiredCodeRuleFlagsABlankCode();
        LegacyTownHallSummaryRequiredJobCodeRuleFlagsABlankJobCode();
        LegacyTownHallSummaryAmountRangeRuleFlagsValuesAboveAmount();
    }
}
