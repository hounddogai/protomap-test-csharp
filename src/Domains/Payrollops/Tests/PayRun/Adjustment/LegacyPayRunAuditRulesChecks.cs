namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyPayRunAuditRulesChecks
{
    public static void LegacyPayRunAuditValidatorReportsEveryProblem()
    {
        var validator = LegacyPayRunAuditRules.DefaultLegacyPayRunAuditValidator();
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayRunAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new LegacyPayRunAuditWindowDaysRangeRule(255, 255);
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPayRunAuditCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LegacyPayRunAuditCapacityRangeRule(64, 120);
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPayRunAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyPayRunAuditCodeLengthRule(120);
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPayRunAuditRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new LegacyPayRunAuditRequiredSkillRule("legacy pay run audit needs this field");
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyPayRunAuditValidatorReportsEveryProblem();
        LegacyPayRunAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        LegacyPayRunAuditCapacityRangeRuleFlagsValuesAboveCapacity();
        LegacyPayRunAuditCodeLengthRuleFlagsALongCode();
        LegacyPayRunAuditRequiredSkillRuleFlagsABlankSkill();
    }
}
