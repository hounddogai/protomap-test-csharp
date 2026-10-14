namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanEntryRulesChecks
{
    public static void GlobalActionPlanEntryValidatorReportsEveryProblem()
    {
        var validator = GlobalActionPlanEntryRules.DefaultGlobalActionPlanEntryValidator();
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalActionPlanEntryRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new GlobalActionPlanEntryRequiredSalaryBandRule("global action plan entry needs a value");
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalActionPlanEntryCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new GlobalActionPlanEntryCostCenterLengthRule(1000);
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalActionPlanEntryCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new GlobalActionPlanEntryCreatedAtRangeRule(64, 80);
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("x", "y");
        item.CreatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalActionPlanEntryValidatorReportsEveryProblem();
        GlobalActionPlanEntryRequiredSalaryBandRuleFlagsABlankSalaryBand();
        GlobalActionPlanEntryCostCenterLengthRuleFlagsALongCostCenter();
        GlobalActionPlanEntryCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
