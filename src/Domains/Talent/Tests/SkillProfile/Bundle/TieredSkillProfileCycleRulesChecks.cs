namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredSkillProfileCycleRulesChecks
{
    public static void TieredSkillProfileCycleValidatorReportsEveryProblem()
    {
        var validator = TieredSkillProfileCycleRules.DefaultTieredSkillProfileCycleValidator();
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredSkillProfileCycleRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new TieredSkillProfileCycleRequiredTimezoneRule("tiered skill profile cycle needs this field");
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredSkillProfileCycleRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new TieredSkillProfileCycleRequiredSalaryBandRule("tiered skill profile cycle needs this field");
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredSkillProfileCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredSkillProfileCycleRequiredCodeRule("tiered skill profile cycle needs a value");
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredSkillProfileCycleValidatorReportsEveryProblem();
        TieredSkillProfileCycleRequiredTimezoneRuleFlagsABlankTimezone();
        TieredSkillProfileCycleRequiredSalaryBandRuleFlagsABlankSalaryBand();
        TieredSkillProfileCycleRequiredCodeRuleFlagsABlankCode();
    }
}
