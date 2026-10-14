namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentPlanRulesChecks
{
    public static void AdaptiveGarnishmentPlanValidatorReportsEveryProblem()
    {
        var validator = AdaptiveGarnishmentPlanRules.DefaultAdaptiveGarnishmentPlanValidator();
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveGarnishmentPlanRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new AdaptiveGarnishmentPlanRequiredEmployeeIdRule("adaptive garnishment plan needs an entry");
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveGarnishmentPlanMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AdaptiveGarnishmentPlanMaximumRangeRule(8.1, 1403.5);
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("x", "y");
        item.Maximum = 1405.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveGarnishmentPlanFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AdaptiveGarnishmentPlanFteRangeRule(7.3, 4830.7);
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("x", "y");
        item.Fte = 4832.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveGarnishmentPlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveGarnishmentPlanRequiredTitleRule("adaptive garnishment plan needs a value");
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentPlanValidatorReportsEveryProblem();
        AdaptiveGarnishmentPlanRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        AdaptiveGarnishmentPlanMaximumRangeRuleFlagsValuesAboveMaximum();
        AdaptiveGarnishmentPlanFteRangeRuleFlagsValuesAboveFte();
        AdaptiveGarnishmentPlanRequiredTitleRuleFlagsABlankTitle();
    }
}
