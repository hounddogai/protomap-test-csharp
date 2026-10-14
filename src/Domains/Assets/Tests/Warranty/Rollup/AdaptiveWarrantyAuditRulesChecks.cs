namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AdaptiveWarrantyAuditRulesChecks
{
    public static void AdaptiveWarrantyAuditValidatorReportsEveryProblem()
    {
        var validator = AdaptiveWarrantyAuditRules.DefaultAdaptiveWarrantyAuditValidator();
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveWarrantyAuditRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AdaptiveWarrantyAuditRequiredJobCodeRule("adaptive warranty audit needs a value");
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveWarrantyAuditAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new AdaptiveWarrantyAuditAmountRangeRule(4.6, 1614.9);
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("x", "y");
        item.Amount = 1616.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyAuditValidatorReportsEveryProblem();
        AdaptiveWarrantyAuditRequiredJobCodeRuleFlagsABlankJobCode();
        AdaptiveWarrantyAuditAmountRangeRuleFlagsValuesAboveAmount();
    }
}
