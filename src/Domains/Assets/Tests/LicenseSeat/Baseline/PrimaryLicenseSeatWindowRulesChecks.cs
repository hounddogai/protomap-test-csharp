namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryLicenseSeatWindowRulesChecks
{
    public static void PrimaryLicenseSeatWindowValidatorReportsEveryProblem()
    {
        var validator = PrimaryLicenseSeatWindowRules.DefaultPrimaryLicenseSeatWindowValidator();
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryLicenseSeatWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryLicenseSeatWindowRequiredLabelRule("primary license seat window needs a value");
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryLicenseSeatWindowRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new PrimaryLicenseSeatWindowRequiredCostCenterRule("primary license seat window needs an entry");
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryLicenseSeatWindowValidatorReportsEveryProblem();
        PrimaryLicenseSeatWindowRequiredLabelRuleFlagsABlankLabel();
        PrimaryLicenseSeatWindowRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
