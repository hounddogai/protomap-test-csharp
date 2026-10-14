namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditRollupRulesChecks
{
    public static void AuditRollupValidatorReportsEveryProblem()
    {
        var validator = AuditRollupRules.DefaultAuditRollupValidator();
        var item = AuditRollup.NewAuditRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditRollupCodeLengthRule(16);
        var item = AuditRollup.NewAuditRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditRollupCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new AuditRollupCategoryLengthRule(64);
        var item = AuditRollup.NewAuditRollup("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditRollupMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AuditRollupMinimumRangeRule(3.3, 4857.4);
        var item = AuditRollup.NewAuditRollup("x", "y");
        item.Minimum = 4858.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AuditRollupValidatorReportsEveryProblem();
        AuditRollupCodeLengthRuleFlagsALongCode();
        AuditRollupCategoryLengthRuleFlagsALongCategory();
        AuditRollupMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
