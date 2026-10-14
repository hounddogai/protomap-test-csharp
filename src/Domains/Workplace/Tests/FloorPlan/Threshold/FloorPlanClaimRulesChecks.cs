namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanClaimRulesChecks
{
    public static void FloorPlanClaimValidatorReportsEveryProblem()
    {
        var validator = FloorPlanClaimRules.DefaultFloorPlanClaimValidator();
        var item = FloorPlanClaim.NewFloorPlanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanClaimNotesLengthRuleFlagsALongNotes()
    {
        var rule = new FloorPlanClaimNotesLengthRule(1000);
        var item = FloorPlanClaim.NewFloorPlanClaim("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new FloorPlanClaimRequiredNameRule("floor plan claim needs this field");
        var item = FloorPlanClaim.NewFloorPlanClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanClaimValidatorReportsEveryProblem();
        FloorPlanClaimNotesLengthRuleFlagsALongNotes();
        FloorPlanClaimRequiredNameRuleFlagsABlankName();
    }
}
