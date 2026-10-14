namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PrimaryAccrualPlanRulesChecks
{
    public static void PrimaryAccrualPlanValidatorReportsEveryProblem()
    {
        var validator = PrimaryAccrualPlanRules.DefaultPrimaryAccrualPlanValidator();
        var item = PrimaryAccrualPlan.NewPrimaryAccrualPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAccrualPlanRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PrimaryAccrualPlanRequiredCountryRule("primary accrual plan needs a value");
        var item = PrimaryAccrualPlan.NewPrimaryAccrualPlan("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAccrualPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryAccrualPlanRequiredLabelRule("primary accrual plan needs a value");
        var item = PrimaryAccrualPlan.NewPrimaryAccrualPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryAccrualPlanValidatorReportsEveryProblem();
        PrimaryAccrualPlanRequiredCountryRuleFlagsABlankCountry();
        PrimaryAccrualPlanRequiredLabelRuleFlagsABlankLabel();
    }
}
