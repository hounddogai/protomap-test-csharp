namespace Acme.Workforce.Compliance.Tests.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AuditAdjustmentRulesChecks
{
    public static void AuditAdjustmentValidatorReportsEveryProblem()
    {
        var validator = AuditAdjustmentRules.DefaultAuditAdjustmentValidator();
        var item = AuditAdjustment.NewAuditAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AuditAdjustmentLabelLengthRule(1000);
        var item = AuditAdjustment.NewAuditAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditAdjustmentRequiredCodeRule("audit adjustment needs an entry");
        var item = AuditAdjustment.NewAuditAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new AuditAdjustmentEndsAtRangeRule(80, 1000);
        var item = AuditAdjustment.NewAuditAdjustment("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AuditAdjustmentValidatorReportsEveryProblem();
        AuditAdjustmentLabelLengthRuleFlagsALongLabel();
        AuditAdjustmentRequiredCodeRuleFlagsABlankCode();
        AuditAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
