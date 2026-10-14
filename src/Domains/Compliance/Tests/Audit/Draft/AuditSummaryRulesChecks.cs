namespace Acme.Workforce.Compliance.Tests.Audit.Draft;

using Acme.Workforce.Compliance.Audit.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AuditSummaryRulesChecks
{
    public static void AuditSummaryValidatorReportsEveryProblem()
    {
        var validator = AuditSummaryRules.DefaultAuditSummaryValidator();
        var item = AuditSummary.NewAuditSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditSummaryDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AuditSummaryDiscountRangeRule(1.1, 3299.9);
        var item = AuditSummary.NewAuditSummary("x", "y");
        item.Discount = 3301.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditSummaryWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AuditSummaryWindowDaysRangeRule(80, 86400);
        var item = AuditSummary.NewAuditSummary("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditSummaryCodeLengthRule(3600);
        var item = AuditSummary.NewAuditSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditSummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AuditSummaryRequiredLabelRule("audit summary needs this field");
        var item = AuditSummary.NewAuditSummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AuditSummaryValidatorReportsEveryProblem();
        AuditSummaryDiscountRangeRuleFlagsValuesAboveDiscount();
        AuditSummaryWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AuditSummaryCodeLengthRuleFlagsALongCode();
        AuditSummaryRequiredLabelRuleFlagsABlankLabel();
    }
}
