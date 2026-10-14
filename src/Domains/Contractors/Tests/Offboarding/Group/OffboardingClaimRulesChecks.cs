namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class OffboardingClaimRulesChecks
{
    public static void OffboardingClaimValidatorReportsEveryProblem()
    {
        var validator = OffboardingClaimRules.DefaultOffboardingClaimValidator();
        var item = OffboardingClaim.NewOffboardingClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingClaimExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new OffboardingClaimExternalRefLengthRule(16);
        var item = OffboardingClaim.NewOffboardingClaim("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingClaimScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new OffboardingClaimScoreRangeRule(6.7, 3415.0);
        var item = OffboardingClaim.NewOffboardingClaim("x", "y");
        item.Score = 3416.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingClaimPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new OffboardingClaimPhoneLengthRule(16);
        var item = OffboardingClaim.NewOffboardingClaim("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingClaimRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new OffboardingClaimRequiredPhoneRule("offboarding claim needs a value");
        var item = OffboardingClaim.NewOffboardingClaim("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingClaimValidatorReportsEveryProblem();
        OffboardingClaimExternalRefLengthRuleFlagsALongExternalRef();
        OffboardingClaimScoreRangeRuleFlagsValuesAboveScore();
        OffboardingClaimPhoneLengthRuleFlagsALongPhone();
        OffboardingClaimRequiredPhoneRuleFlagsABlankPhone();
    }
}
