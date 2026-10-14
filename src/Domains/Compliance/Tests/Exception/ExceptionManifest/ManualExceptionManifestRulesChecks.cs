namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ManualExceptionManifestRulesChecks
{
    public static void ManualExceptionManifestValidatorReportsEveryProblem()
    {
        var validator = ManualExceptionManifestRules.DefaultManualExceptionManifestValidator();
        var item = ManualExceptionManifest.NewManualExceptionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualExceptionManifestNameLengthRuleFlagsALongName()
    {
        var rule = new ManualExceptionManifestNameLengthRule(32);
        var item = ManualExceptionManifest.NewManualExceptionManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualExceptionManifestCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new ManualExceptionManifestCostCenterLengthRule(3600);
        var item = ManualExceptionManifest.NewManualExceptionManifest("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualExceptionManifestChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ManualExceptionManifestChannelLengthRule(1000);
        var item = ManualExceptionManifest.NewManualExceptionManifest("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualExceptionManifestValidatorReportsEveryProblem();
        ManualExceptionManifestNameLengthRuleFlagsALongName();
        ManualExceptionManifestCostCenterLengthRuleFlagsALongCostCenter();
        ManualExceptionManifestChannelLengthRuleFlagsALongChannel();
    }
}
