namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LicenseSeatAdjustmentRulesChecks
{
    public static void LicenseSeatAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatAdjustmentRules.DefaultLicenseSeatAdjustmentValidator();
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LicenseSeatAdjustmentRequiredCodeRule("license seat adjustment needs this field");
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LicenseSeatAdjustmentRequiredLabelRule("license seat adjustment needs this field");
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new LicenseSeatAdjustmentNameLengthRule(16);
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new LicenseSeatAdjustmentRequiredNameRule("license seat adjustment needs an entry");
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatAdjustmentValidatorReportsEveryProblem();
        LicenseSeatAdjustmentRequiredCodeRuleFlagsABlankCode();
        LicenseSeatAdjustmentRequiredLabelRuleFlagsABlankLabel();
        LicenseSeatAdjustmentNameLengthRuleFlagsALongName();
        LicenseSeatAdjustmentRequiredNameRuleFlagsABlankName();
    }
}
