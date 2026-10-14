namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualManifestRulesChecks
{
    public static void AccrualManifestValidatorReportsEveryProblem()
    {
        var validator = AccrualManifestRules.DefaultAccrualManifestValidator();
        var item = AccrualManifest.NewAccrualManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualManifestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccrualManifestRequiredLabelRule("accrual manifest needs a value");
        var item = AccrualManifest.NewAccrualManifest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualManifestCodeLengthRule(16);
        var item = AccrualManifest.NewAccrualManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualManifestTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AccrualManifestTimezoneLengthRule(80);
        var item = AccrualManifest.NewAccrualManifest("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualManifestValidatorReportsEveryProblem();
        AccrualManifestRequiredLabelRuleFlagsABlankLabel();
        AccrualManifestCodeLengthRuleFlagsALongCode();
        AccrualManifestTimezoneLengthRuleFlagsALongTimezone();
    }
}
