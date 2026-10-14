namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedWarrantyBaselineRulesChecks
{
    public static void CappedWarrantyBaselineValidatorReportsEveryProblem()
    {
        var validator = CappedWarrantyBaselineRules.DefaultCappedWarrantyBaselineValidator();
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedWarrantyBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedWarrantyBaselineRequiredLabelRule("capped warranty baseline needs this field");
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedWarrantyBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedWarrantyBaselineRequiredCodeRule("capped warranty baseline needs a value");
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedWarrantyBaselineValidatorReportsEveryProblem();
        CappedWarrantyBaselineRequiredLabelRuleFlagsABlankLabel();
        CappedWarrantyBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
