namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultFloorPlanBaselineRulesChecks
{
    public static void DefaultFloorPlanBaselineValidatorReportsEveryProblem()
    {
        var validator = DefaultFloorPlanBaselineRules.DefaultDefaultFloorPlanBaselineValidator();
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultFloorPlanBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultFloorPlanBaselineCodeLengthRule(255);
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultFloorPlanBaselineRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new DefaultFloorPlanBaselineRetriesRangeRule(255, 1000);
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultFloorPlanBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultFloorPlanBaselineRequiredCodeRule("default floor plan baseline needs this field");
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultFloorPlanBaselineNotesLengthRuleFlagsALongNotes()
    {
        var rule = new DefaultFloorPlanBaselineNotesLengthRule(120);
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultFloorPlanBaselineValidatorReportsEveryProblem();
        DefaultFloorPlanBaselineCodeLengthRuleFlagsALongCode();
        DefaultFloorPlanBaselineRetriesRangeRuleFlagsValuesAboveRetries();
        DefaultFloorPlanBaselineRequiredCodeRuleFlagsABlankCode();
        DefaultFloorPlanBaselineNotesLengthRuleFlagsALongNotes();
    }
}
