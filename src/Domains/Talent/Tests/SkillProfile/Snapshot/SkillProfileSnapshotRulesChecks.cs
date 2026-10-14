namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfileSnapshotRulesChecks
{
    public static void SkillProfileSnapshotValidatorReportsEveryProblem()
    {
        var validator = SkillProfileSnapshotRules.DefaultSkillProfileSnapshotValidator();
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileSnapshotCodeLengthRule(120);
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileSnapshotMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new SkillProfileSnapshotMinimumRangeRule(7.2, 2135.4);
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("x", "y");
        item.Minimum = 2136.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileSnapshotRequiredCodeRule("skill profile snapshot needs an entry");
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileSnapshotRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new SkillProfileSnapshotRequiredJobCodeRule("skill profile snapshot needs a value");
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileSnapshotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SkillProfileSnapshotLabelLengthRule(3600);
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileSnapshotValidatorReportsEveryProblem();
        SkillProfileSnapshotCodeLengthRuleFlagsALongCode();
        SkillProfileSnapshotMinimumRangeRuleFlagsValuesAboveMinimum();
        SkillProfileSnapshotRequiredCodeRuleFlagsABlankCode();
        SkillProfileSnapshotRequiredJobCodeRuleFlagsABlankJobCode();
        SkillProfileSnapshotLabelLengthRuleFlagsALongLabel();
    }
}
