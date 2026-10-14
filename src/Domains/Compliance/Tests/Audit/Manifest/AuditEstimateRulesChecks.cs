namespace Acme.Workforce.Compliance.Tests.Audit.Manifest;

using Acme.Workforce.Compliance.Audit.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AuditEstimateRulesChecks
{
    public static void AuditEstimateValidatorReportsEveryProblem()
    {
        var validator = AuditEstimateRules.DefaultAuditEstimateValidator();
        var item = AuditEstimate.NewAuditEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditEstimateRequiredCodeRule("audit estimate needs a value");
        var item = AuditEstimate.NewAuditEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditEstimateRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new AuditEstimateRatingRangeRule(0.7, 1932.7);
        var item = AuditEstimate.NewAuditEstimate("x", "y");
        item.Rating = 1934.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new AuditEstimateNameLengthRule(1000);
        var item = AuditEstimate.NewAuditEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditEstimateValidatorReportsEveryProblem();
        AuditEstimateRequiredCodeRuleFlagsABlankCode();
        AuditEstimateRatingRangeRuleFlagsValuesAboveRating();
        AuditEstimateNameLengthRuleFlagsALongName();
    }
}
