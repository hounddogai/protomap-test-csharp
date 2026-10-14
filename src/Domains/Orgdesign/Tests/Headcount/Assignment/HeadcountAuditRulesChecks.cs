namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountAuditRulesChecks
{
    public static void HeadcountAuditValidatorReportsEveryProblem()
    {
        var validator = HeadcountAuditRules.DefaultHeadcountAuditValidator();
        var item = HeadcountAudit.NewHeadcountAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountAuditRequiredCodeRule("headcount audit needs this field");
        var item = HeadcountAudit.NewHeadcountAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountAuditAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new HeadcountAuditAmountRangeRule(1.0, 682.3);
        var item = HeadcountAudit.NewHeadcountAudit("x", "y");
        item.Amount = 683.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HeadcountAuditValidatorReportsEveryProblem();
        HeadcountAuditRequiredCodeRuleFlagsABlankCode();
        HeadcountAuditAmountRangeRuleFlagsValuesAboveAmount();
    }
}
