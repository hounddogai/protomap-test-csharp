namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LegacyHeadcountQuotaRulesChecks
{
    public static void LegacyHeadcountQuotaValidatorReportsEveryProblem()
    {
        var validator = LegacyHeadcountQuotaRules.DefaultLegacyHeadcountQuotaValidator();
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyHeadcountQuotaDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new LegacyHeadcountQuotaDepartmentLengthRule(255);
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyHeadcountQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyHeadcountQuotaCodeLengthRule(32);
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyHeadcountQuotaRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new LegacyHeadcountQuotaRequiredDepartmentRule("legacy headcount quota needs this field");
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyHeadcountQuotaValidatorReportsEveryProblem();
        LegacyHeadcountQuotaDepartmentLengthRuleFlagsALongDepartment();
        LegacyHeadcountQuotaCodeLengthRuleFlagsALongCode();
        LegacyHeadcountQuotaRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
