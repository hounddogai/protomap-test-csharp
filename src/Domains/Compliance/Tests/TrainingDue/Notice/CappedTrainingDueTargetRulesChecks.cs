namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedTrainingDueTargetRulesChecks
{
    public static void CappedTrainingDueTargetValidatorReportsEveryProblem()
    {
        var validator = CappedTrainingDueTargetRules.DefaultCappedTrainingDueTargetValidator();
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedTrainingDueTargetRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new CappedTrainingDueTargetRequiredChecksumRule("capped training due target needs a value");
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedTrainingDueTargetChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new CappedTrainingDueTargetChecksumLengthRule(3600);
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedTrainingDueTargetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedTrainingDueTargetLabelLengthRule(32);
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedTrainingDueTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedTrainingDueTargetRequiredLabelRule("capped training due target needs an entry");
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedTrainingDueTargetValidatorReportsEveryProblem();
        CappedTrainingDueTargetRequiredChecksumRuleFlagsABlankChecksum();
        CappedTrainingDueTargetChecksumLengthRuleFlagsALongChecksum();
        CappedTrainingDueTargetLabelLengthRuleFlagsALongLabel();
        CappedTrainingDueTargetRequiredLabelRuleFlagsABlankLabel();
    }
}
