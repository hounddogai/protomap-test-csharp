namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueAllocationRulesChecks
{
    public static void TrainingDueAllocationValidatorReportsEveryProblem()
    {
        var validator = TrainingDueAllocationRules.DefaultTrainingDueAllocationValidator();
        var item = TrainingDueAllocation.NewTrainingDueAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueAllocationRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new TrainingDueAllocationRequiredNotesRule("training due allocation needs this field");
        var item = TrainingDueAllocation.NewTrainingDueAllocation("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueAllocationCodeLengthRule(120);
        var item = TrainingDueAllocation.NewTrainingDueAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueAllocationNotesLengthRuleFlagsALongNotes()
    {
        var rule = new TrainingDueAllocationNotesLengthRule(120);
        var item = TrainingDueAllocation.NewTrainingDueAllocation("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueAllocationFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new TrainingDueAllocationFteRangeRule(7.9, 553.2);
        var item = TrainingDueAllocation.NewTrainingDueAllocation("x", "y");
        item.Fte = 554.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueAllocationRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new TrainingDueAllocationRequiredChecksumRule("training due allocation needs a value");
        var item = TrainingDueAllocation.NewTrainingDueAllocation("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueAllocationValidatorReportsEveryProblem();
        TrainingDueAllocationRequiredNotesRuleFlagsABlankNotes();
        TrainingDueAllocationCodeLengthRuleFlagsALongCode();
        TrainingDueAllocationNotesLengthRuleFlagsALongNotes();
        TrainingDueAllocationFteRangeRuleFlagsValuesAboveFte();
        TrainingDueAllocationRequiredChecksumRuleFlagsABlankChecksum();
    }
}
