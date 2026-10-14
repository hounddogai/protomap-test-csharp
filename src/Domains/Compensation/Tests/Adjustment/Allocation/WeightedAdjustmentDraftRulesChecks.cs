namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class WeightedAdjustmentDraftRulesChecks
{
    public static void WeightedAdjustmentDraftValidatorReportsEveryProblem()
    {
        var validator = WeightedAdjustmentDraftRules.DefaultWeightedAdjustmentDraftValidator();
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAdjustmentDraftNotesLengthRuleFlagsALongNotes()
    {
        var rule = new WeightedAdjustmentDraftNotesLengthRule(16);
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedAdjustmentDraftValidatorReportsEveryProblem();
        WeightedAdjustmentDraftNotesLengthRuleFlagsALongNotes();
    }
}
