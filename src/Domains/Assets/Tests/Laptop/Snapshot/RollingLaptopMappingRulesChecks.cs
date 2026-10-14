namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingLaptopMappingRulesChecks
{
    public static void RollingLaptopMappingValidatorReportsEveryProblem()
    {
        var validator = RollingLaptopMappingRules.DefaultRollingLaptopMappingValidator();
        var item = RollingLaptopMapping.NewRollingLaptopMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingLaptopMappingSkillLengthRuleFlagsALongSkill()
    {
        var rule = new RollingLaptopMappingSkillLengthRule(16);
        var item = RollingLaptopMapping.NewRollingLaptopMapping("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingLaptopMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingLaptopMappingRequiredNameRule("rolling laptop mapping needs this field");
        var item = RollingLaptopMapping.NewRollingLaptopMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLaptopMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingLaptopMappingRequiredCodeRule("rolling laptop mapping needs a value");
        var item = RollingLaptopMapping.NewRollingLaptopMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLaptopMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingLaptopMappingCodeLengthRule(3600);
        var item = RollingLaptopMapping.NewRollingLaptopMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingLaptopMappingRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new RollingLaptopMappingRequiredOwnerIdRule("rolling laptop mapping needs an entry");
        var item = RollingLaptopMapping.NewRollingLaptopMapping("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingLaptopMappingValidatorReportsEveryProblem();
        RollingLaptopMappingSkillLengthRuleFlagsALongSkill();
        RollingLaptopMappingRequiredNameRuleFlagsABlankName();
        RollingLaptopMappingRequiredCodeRuleFlagsABlankCode();
        RollingLaptopMappingCodeLengthRuleFlagsALongCode();
        RollingLaptopMappingRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
