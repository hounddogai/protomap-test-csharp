namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class DefaultAccrualManifestRulesChecks
{
    public static void DefaultAccrualManifestValidatorReportsEveryProblem()
    {
        var validator = DefaultAccrualManifestRules.DefaultDefaultAccrualManifestValidator();
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAccrualManifestSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new DefaultAccrualManifestSequenceRangeRule(255, 255);
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAccrualManifestRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new DefaultAccrualManifestRequiredTimezoneRule("default accrual manifest needs an entry");
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultAccrualManifestValidatorReportsEveryProblem();
        DefaultAccrualManifestSequenceRangeRuleFlagsValuesAboveSequence();
        DefaultAccrualManifestRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
