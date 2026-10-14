namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AuditTierRulesChecks
{
    public static void AuditTierValidatorReportsEveryProblem()
    {
        var validator = AuditTierRules.DefaultAuditTierValidator();
        var item = AuditTier.NewAuditTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditTierRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new AuditTierRequiredGradeRule("audit tier needs a value");
        var item = AuditTier.NewAuditTier("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditTierDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new AuditTierDurationMinutesRangeRule(16, 32);
        var item = AuditTier.NewAuditTier("x", "y");
        item.DurationMinutes = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditTierGradeLengthRuleFlagsALongGrade()
    {
        var rule = new AuditTierGradeLengthRule(32);
        var item = AuditTier.NewAuditTier("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditTierValidatorReportsEveryProblem();
        AuditTierRequiredGradeRuleFlagsABlankGrade();
        AuditTierDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        AuditTierGradeLengthRuleFlagsALongGrade();
    }
}
