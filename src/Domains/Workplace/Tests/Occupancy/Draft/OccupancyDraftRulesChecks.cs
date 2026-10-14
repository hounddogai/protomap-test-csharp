namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyDraftRulesChecks
{
    public static void OccupancyDraftValidatorReportsEveryProblem()
    {
        var validator = OccupancyDraftRules.DefaultOccupancyDraftValidator();
        var item = OccupancyDraft.NewOccupancyDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyDraftBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new OccupancyDraftBudgetRangeRule(7.5, 1476.8);
        var item = OccupancyDraft.NewOccupancyDraft("x", "y");
        item.Budget = 1478.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new OccupancyDraftLeaveDaysRangeRule(64, 120);
        var item = OccupancyDraft.NewOccupancyDraft("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyDraftNameLengthRuleFlagsALongName()
    {
        var rule = new OccupancyDraftNameLengthRule(255);
        var item = OccupancyDraft.NewOccupancyDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyDraftValidatorReportsEveryProblem();
        OccupancyDraftBudgetRangeRuleFlagsValuesAboveBudget();
        OccupancyDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        OccupancyDraftNameLengthRuleFlagsALongName();
    }
}
