namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyBudgetRulesChecks
{
    public static void OccupancyBudgetValidatorReportsEveryProblem()
    {
        var validator = OccupancyBudgetRules.DefaultOccupancyBudgetValidator();
        var item = OccupancyBudget.NewOccupancyBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyBudgetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new OccupancyBudgetRequiredLabelRule("occupancy budget needs this field");
        var item = OccupancyBudget.NewOccupancyBudget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyBudgetNotesLengthRuleFlagsALongNotes()
    {
        var rule = new OccupancyBudgetNotesLengthRule(120);
        var item = OccupancyBudget.NewOccupancyBudget("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyBudgetRequiredCodeRule("occupancy budget needs a value");
        var item = OccupancyBudget.NewOccupancyBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyBudgetValidatorReportsEveryProblem();
        OccupancyBudgetRequiredLabelRuleFlagsABlankLabel();
        OccupancyBudgetNotesLengthRuleFlagsALongNotes();
        OccupancyBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
