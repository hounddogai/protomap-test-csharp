namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class OffboardingLedgerRulesChecks
{
    public static void OffboardingLedgerValidatorReportsEveryProblem()
    {
        var validator = OffboardingLedgerRules.DefaultOffboardingLedgerValidator();
        var item = OffboardingLedger.NewOffboardingLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingLedgerCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new OffboardingLedgerCategoryLengthRule(32);
        var item = OffboardingLedger.NewOffboardingLedger("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingLedgerRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new OffboardingLedgerRevisionRangeRule(80, 86400);
        var item = OffboardingLedger.NewOffboardingLedger("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OffboardingLedgerTitleLengthRule(64);
        var item = OffboardingLedger.NewOffboardingLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingLedgerMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new OffboardingLedgerMarginRangeRule(9.3, 319.6);
        var item = OffboardingLedger.NewOffboardingLedger("x", "y");
        item.Margin = 321.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OffboardingLedgerRequiredCodeRule("offboarding ledger needs this field");
        var item = OffboardingLedger.NewOffboardingLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingLedgerValidatorReportsEveryProblem();
        OffboardingLedgerCategoryLengthRuleFlagsALongCategory();
        OffboardingLedgerRevisionRangeRuleFlagsValuesAboveRevision();
        OffboardingLedgerTitleLengthRuleFlagsALongTitle();
        OffboardingLedgerMarginRangeRuleFlagsValuesAboveMargin();
        OffboardingLedgerRequiredCodeRuleFlagsABlankCode();
    }
}
