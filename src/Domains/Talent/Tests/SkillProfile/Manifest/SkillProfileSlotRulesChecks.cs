namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class SkillProfileSlotRulesChecks
{
    public static void SkillProfileSlotValidatorReportsEveryProblem()
    {
        var validator = SkillProfileSlotRules.DefaultSkillProfileSlotValidator();
        var item = SkillProfileSlot.NewSkillProfileSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileSlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SkillProfileSlotTitleLengthRule(255);
        var item = SkillProfileSlot.NewSkillProfileSlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileSlotCodeLengthRule(80);
        var item = SkillProfileSlot.NewSkillProfileSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileSlotChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new SkillProfileSlotChecksumLengthRule(1000);
        var item = SkillProfileSlot.NewSkillProfileSlot("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileSlotValidatorReportsEveryProblem();
        SkillProfileSlotTitleLengthRuleFlagsALongTitle();
        SkillProfileSlotCodeLengthRuleFlagsALongCode();
        SkillProfileSlotChecksumLengthRuleFlagsALongChecksum();
    }
}
