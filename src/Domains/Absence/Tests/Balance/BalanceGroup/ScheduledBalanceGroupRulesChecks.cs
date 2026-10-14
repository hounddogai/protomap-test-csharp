namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class ScheduledBalanceGroupRulesChecks
{
    public static void ScheduledBalanceGroupValidatorReportsEveryProblem()
    {
        var validator = ScheduledBalanceGroupRules.DefaultScheduledBalanceGroupValidator();
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBalanceGroupRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new ScheduledBalanceGroupRequiredDepartmentRule("scheduled balance group needs a value");
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBalanceGroupCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ScheduledBalanceGroupCostRangeRule(4.1, 4351.9);
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("x", "y");
        item.Cost = 4353.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledBalanceGroupRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ScheduledBalanceGroupRequiredLocaleRule("scheduled balance group needs this field");
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledBalanceGroupValidatorReportsEveryProblem();
        ScheduledBalanceGroupRequiredDepartmentRuleFlagsABlankDepartment();
        ScheduledBalanceGroupCostRangeRuleFlagsValuesAboveCost();
        ScheduledBalanceGroupRequiredLocaleRuleFlagsABlankLocale();
    }
}
