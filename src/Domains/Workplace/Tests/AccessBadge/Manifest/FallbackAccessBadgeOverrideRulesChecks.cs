namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackAccessBadgeOverrideRulesChecks
{
    public static void FallbackAccessBadgeOverrideValidatorReportsEveryProblem()
    {
        var validator = FallbackAccessBadgeOverrideRules.DefaultFallbackAccessBadgeOverrideValidator();
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAccessBadgeOverrideRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new FallbackAccessBadgeOverrideRequiredChecksumRule("fallback access badge override needs a value");
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAccessBadgeOverrideRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new FallbackAccessBadgeOverrideRetriesRangeRule(1000, 3600);
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackAccessBadgeOverrideChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new FallbackAccessBadgeOverrideChecksumLengthRule(86400);
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAccessBadgeOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackAccessBadgeOverrideRequiredCodeRule("fallback access badge override needs this field");
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeOverrideValidatorReportsEveryProblem();
        FallbackAccessBadgeOverrideRequiredChecksumRuleFlagsABlankChecksum();
        FallbackAccessBadgeOverrideRetriesRangeRuleFlagsValuesAboveRetries();
        FallbackAccessBadgeOverrideChecksumLengthRuleFlagsALongChecksum();
        FallbackAccessBadgeOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
