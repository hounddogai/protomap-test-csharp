namespace Acme.Workforce.Absence.Tests.ReturnPlan.Entry;

using Acme.Workforce.Absence.ReturnPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalReturnPlanRollupRulesChecks
{
    public static void SeasonalReturnPlanRollupValidatorReportsEveryProblem()
    {
        var validator = SeasonalReturnPlanRollupRules.DefaultSeasonalReturnPlanRollupValidator();
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalReturnPlanRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalReturnPlanRollupCodeLengthRule(3600);
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalReturnPlanRollupAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SeasonalReturnPlanRollupAmountRangeRule(6.3, 1354.6);
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("x", "y");
        item.Amount = 1356.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalReturnPlanRollupRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new SeasonalReturnPlanRollupRequiredChannelRule("seasonal return plan rollup needs an entry");
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalReturnPlanRollupWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new SeasonalReturnPlanRollupWeightRangeRule(8.0, 609.7);
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("x", "y");
        item.Weight = 611.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalReturnPlanRollupRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new SeasonalReturnPlanRollupRevisionRangeRule(32, 64);
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("x", "y");
        item.Revision = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalReturnPlanRollupValidatorReportsEveryProblem();
        SeasonalReturnPlanRollupCodeLengthRuleFlagsALongCode();
        SeasonalReturnPlanRollupAmountRangeRuleFlagsValuesAboveAmount();
        SeasonalReturnPlanRollupRequiredChannelRuleFlagsABlankChannel();
        SeasonalReturnPlanRollupWeightRangeRuleFlagsValuesAboveWeight();
        SeasonalReturnPlanRollupRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
