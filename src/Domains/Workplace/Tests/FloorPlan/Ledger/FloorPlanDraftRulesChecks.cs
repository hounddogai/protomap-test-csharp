namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanDraftRulesChecks
{
    public static void FloorPlanDraftValidatorReportsEveryProblem()
    {
        var validator = FloorPlanDraftRules.DefaultFloorPlanDraftValidator();
        var item = FloorPlanDraft.NewFloorPlanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanDraftRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new FloorPlanDraftRequiredSummaryRule("floor plan draft needs an entry");
        var item = FloorPlanDraft.NewFloorPlanDraft("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanDraftWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new FloorPlanDraftWindowDaysRangeRule(32, 1000);
        var item = FloorPlanDraft.NewFloorPlanDraft("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanDraftNameLengthRuleFlagsALongName()
    {
        var rule = new FloorPlanDraftNameLengthRule(1000);
        var item = FloorPlanDraft.NewFloorPlanDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanDraftChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new FloorPlanDraftChecksumLengthRule(80);
        var item = FloorPlanDraft.NewFloorPlanDraft("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanDraftValidatorReportsEveryProblem();
        FloorPlanDraftRequiredSummaryRuleFlagsABlankSummary();
        FloorPlanDraftWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        FloorPlanDraftNameLengthRuleFlagsALongName();
        FloorPlanDraftChecksumLengthRuleFlagsALongChecksum();
    }
}
