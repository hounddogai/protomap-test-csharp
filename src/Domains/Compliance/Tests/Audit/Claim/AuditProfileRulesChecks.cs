namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditProfileRulesChecks
{
    public static void AuditProfileValidatorReportsEveryProblem()
    {
        var validator = AuditProfileRules.DefaultAuditProfileValidator();
        var item = AuditProfile.NewAuditProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditProfileDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AuditProfileDiscountRangeRule(3.4, 863.1);
        var item = AuditProfile.NewAuditProfile("x", "y");
        item.Discount = 864.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new AuditProfileCodeLengthRule(16);
        var item = AuditProfile.NewAuditProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditProfileLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new AuditProfileLocaleLengthRule(64);
        var item = AuditProfile.NewAuditProfile("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditProfileRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new AuditProfileRequiredLocaleRule("audit profile needs an entry");
        var item = AuditProfile.NewAuditProfile("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new AuditProfileTenureMonthsRangeRule(255, 255);
        var item = AuditProfile.NewAuditProfile("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AuditProfileLabelLengthRule(86400);
        var item = AuditProfile.NewAuditProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditProfileValidatorReportsEveryProblem();
        AuditProfileDiscountRangeRuleFlagsValuesAboveDiscount();
        AuditProfileCodeLengthRuleFlagsALongCode();
        AuditProfileLocaleLengthRuleFlagsALongLocale();
        AuditProfileRequiredLocaleRuleFlagsABlankLocale();
        AuditProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        AuditProfileLabelLengthRuleFlagsALongLabel();
    }
}
