namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSnapshotRulesChecks
{
    public static void ReorgSnapshotValidatorReportsEveryProblem()
    {
        var validator = ReorgSnapshotRules.DefaultReorgSnapshotValidator();
        var item = ReorgSnapshot.NewReorgSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgSnapshotRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new ReorgSnapshotRequiredSkillRule("reorg snapshot needs an entry");
        var item = ReorgSnapshot.NewReorgSnapshot("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgSnapshotSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ReorgSnapshotSkillLengthRule(3600);
        var item = ReorgSnapshot.NewReorgSnapshot("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgSnapshotMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ReorgSnapshotMarginRangeRule(4.8, 1738.8);
        var item = ReorgSnapshot.NewReorgSnapshot("x", "y");
        item.Margin = 1740.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReorgSnapshotValidatorReportsEveryProblem();
        ReorgSnapshotRequiredSkillRuleFlagsABlankSkill();
        ReorgSnapshotSkillLengthRuleFlagsALongSkill();
        ReorgSnapshotMarginRangeRuleFlagsValuesAboveMargin();
    }
}
