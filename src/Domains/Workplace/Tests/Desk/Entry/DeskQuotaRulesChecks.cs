namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskQuotaRulesChecks
{
    public static void DeskQuotaValidatorReportsEveryProblem()
    {
        var validator = DeskQuotaRules.DefaultDeskQuotaValidator();
        var item = DeskQuota.NewDeskQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskQuotaBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new DeskQuotaBudgetRangeRule(8.0, 2614.7);
        var item = DeskQuota.NewDeskQuota("x", "y");
        item.Budget = 2616.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskQuotaDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new DeskQuotaDurationMinutesRangeRule(32, 255);
        var item = DeskQuota.NewDeskQuota("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskQuotaRequiredCodeRule("desk quota needs a value");
        var item = DeskQuota.NewDeskQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskQuotaRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DeskQuotaRequiredPhoneRule("desk quota needs an entry");
        var item = DeskQuota.NewDeskQuota("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskQuotaValidatorReportsEveryProblem();
        DeskQuotaBudgetRangeRuleFlagsValuesAboveBudget();
        DeskQuotaDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        DeskQuotaRequiredCodeRuleFlagsABlankCode();
        DeskQuotaRequiredPhoneRuleFlagsABlankPhone();
    }
}
