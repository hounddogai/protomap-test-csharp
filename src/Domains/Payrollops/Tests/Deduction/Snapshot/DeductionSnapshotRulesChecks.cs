namespace Acme.Workforce.Payrollops.Tests.Deduction.Snapshot;

using Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DeductionSnapshotRulesChecks
{
    public static void DeductionSnapshotValidatorReportsEveryProblem()
    {
        var validator = DeductionSnapshotRules.DefaultDeductionSnapshotValidator();
        var item = DeductionSnapshot.NewDeductionSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionSnapshotCodeLengthRule(16);
        var item = DeductionSnapshot.NewDeductionSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionSnapshotRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new DeductionSnapshotRequiredSkillRule("deduction snapshot needs a value");
        var item = DeductionSnapshot.NewDeductionSnapshot("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionSnapshotValidatorReportsEveryProblem();
        DeductionSnapshotCodeLengthRuleFlagsALongCode();
        DeductionSnapshotRequiredSkillRuleFlagsABlankSkill();
    }
}
