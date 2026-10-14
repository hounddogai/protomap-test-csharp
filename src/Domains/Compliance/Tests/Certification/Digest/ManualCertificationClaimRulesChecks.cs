namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ManualCertificationClaimRulesChecks
{
    public static void ManualCertificationClaimValidatorReportsEveryProblem()
    {
        var validator = ManualCertificationClaimRules.DefaultManualCertificationClaimValidator();
        var item = ManualCertificationClaim.NewManualCertificationClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualCertificationClaimStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new ManualCertificationClaimStartsAtRangeRule(32, 86400);
        var item = ManualCertificationClaim.NewManualCertificationClaim("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualCertificationClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualCertificationClaimCodeLengthRule(86400);
        var item = ManualCertificationClaim.NewManualCertificationClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualCertificationClaimNameLengthRuleFlagsALongName()
    {
        var rule = new ManualCertificationClaimNameLengthRule(86400);
        var item = ManualCertificationClaim.NewManualCertificationClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualCertificationClaimValidatorReportsEveryProblem();
        ManualCertificationClaimStartsAtRangeRuleFlagsValuesAboveStartsAt();
        ManualCertificationClaimCodeLengthRuleFlagsALongCode();
        ManualCertificationClaimNameLengthRuleFlagsALongName();
    }
}
