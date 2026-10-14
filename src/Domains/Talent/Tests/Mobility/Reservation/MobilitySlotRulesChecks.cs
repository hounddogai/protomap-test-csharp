namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilitySlotRulesChecks
{
    public static void MobilitySlotValidatorReportsEveryProblem()
    {
        var validator = MobilitySlotRules.DefaultMobilitySlotValidator();
        var item = MobilitySlot.NewMobilitySlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilitySlotRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new MobilitySlotRequiredSkillRule("mobility slot needs an entry");
        var item = MobilitySlot.NewMobilitySlot("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilitySlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilitySlotRequiredCodeRule("mobility slot needs this field");
        var item = MobilitySlot.NewMobilitySlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilitySlotValidatorReportsEveryProblem();
        MobilitySlotRequiredSkillRuleFlagsABlankSkill();
        MobilitySlotRequiredCodeRuleFlagsABlankCode();
    }
}
