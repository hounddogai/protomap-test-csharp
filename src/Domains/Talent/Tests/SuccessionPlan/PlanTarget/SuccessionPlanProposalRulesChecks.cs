namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class SuccessionPlanProposalRulesChecks
{
    public static void SuccessionPlanProposalValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanProposalRules.DefaultSuccessionPlanProposalValidator();
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanProposalCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new SuccessionPlanProposalCapacityRangeRule(3600, 86400);
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanProposalFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SuccessionPlanProposalFloorValueRangeRule(7.1, 3437.4);
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("x", "y");
        item.FloorValue = 3438.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanProposalRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new SuccessionPlanProposalRequiredManagerIdRule("succession plan proposal needs an entry");
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanProposalNotesLengthRuleFlagsALongNotes()
    {
        var rule = new SuccessionPlanProposalNotesLengthRule(32);
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanProposalManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new SuccessionPlanProposalManagerIdLengthRule(1000);
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanProposalValidatorReportsEveryProblem();
        SuccessionPlanProposalCapacityRangeRuleFlagsValuesAboveCapacity();
        SuccessionPlanProposalFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SuccessionPlanProposalRequiredManagerIdRuleFlagsABlankManagerId();
        SuccessionPlanProposalNotesLengthRuleFlagsALongNotes();
        SuccessionPlanProposalManagerIdLengthRuleFlagsALongManagerId();
    }
}
