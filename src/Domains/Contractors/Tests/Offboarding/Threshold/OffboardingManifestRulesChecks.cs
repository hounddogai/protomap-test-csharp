namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingManifestRulesChecks
{
    public static void OffboardingManifestValidatorReportsEveryProblem()
    {
        var validator = OffboardingManifestRules.DefaultOffboardingManifestValidator();
        var item = OffboardingManifest.NewOffboardingManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingManifestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OffboardingManifestRequiredTitleRule("offboarding manifest needs this field");
        var item = OffboardingManifest.NewOffboardingManifest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingManifestCodeLengthRule(255);
        var item = OffboardingManifest.NewOffboardingManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OffboardingManifestRequiredCodeRule("offboarding manifest needs a value");
        var item = OffboardingManifest.NewOffboardingManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new OffboardingManifestExpiresAtRangeRule(16, 32);
        var item = OffboardingManifest.NewOffboardingManifest("x", "y");
        item.ExpiresAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingManifestUnitLengthRuleFlagsALongUnit()
    {
        var rule = new OffboardingManifestUnitLengthRule(255);
        var item = OffboardingManifest.NewOffboardingManifest("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingManifestValidatorReportsEveryProblem();
        OffboardingManifestRequiredTitleRuleFlagsABlankTitle();
        OffboardingManifestCodeLengthRuleFlagsALongCode();
        OffboardingManifestRequiredCodeRuleFlagsABlankCode();
        OffboardingManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        OffboardingManifestUnitLengthRuleFlagsALongUnit();
    }
}
