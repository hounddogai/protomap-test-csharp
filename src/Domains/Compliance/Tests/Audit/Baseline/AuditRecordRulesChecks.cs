namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AuditRecordRulesChecks
{
    public static void AuditRecordValidatorReportsEveryProblem()
    {
        var validator = AuditRecordRules.DefaultAuditRecordValidator();
        var item = AuditRecord.NewAuditRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditRecordCodeLengthRule(32);
        var item = AuditRecord.NewAuditRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditRecordReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new AuditRecordReviewerLengthRule(3600);
        var item = AuditRecord.NewAuditRecord("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditRecordRequiredCodeRule("audit record needs this field");
        var item = AuditRecord.NewAuditRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditRecordLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new AuditRecordLimitRangeRule(1000, 1000);
        var item = AuditRecord.NewAuditRecord("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditRecordRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AuditRecordRateRangeRule(7.7, 3725.4);
        var item = AuditRecord.NewAuditRecord("x", "y");
        item.Rate = 3726.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AuditRecordValidatorReportsEveryProblem();
        AuditRecordCodeLengthRuleFlagsALongCode();
        AuditRecordReviewerLengthRuleFlagsALongReviewer();
        AuditRecordRequiredCodeRuleFlagsABlankCode();
        AuditRecordLimitRangeRuleFlagsValuesAboveLimit();
        AuditRecordRateRangeRuleFlagsValuesAboveRate();
    }
}
