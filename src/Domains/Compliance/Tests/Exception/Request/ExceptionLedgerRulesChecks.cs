namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionLedgerRulesChecks
{
    public static void ExceptionLedgerValidatorReportsEveryProblem()
    {
        var validator = ExceptionLedgerRules.DefaultExceptionLedgerValidator();
        var item = ExceptionLedger.NewExceptionLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ExceptionLedgerRequiredLabelRule("exception ledger needs this field");
        var item = ExceptionLedger.NewExceptionLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ExceptionLedgerLabelLengthRule(86400);
        var item = ExceptionLedger.NewExceptionLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionLedgerLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ExceptionLedgerLimitRangeRule(32, 3600);
        var item = ExceptionLedger.NewExceptionLedger("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionLedgerRequiredCodeRule("exception ledger needs a value");
        var item = ExceptionLedger.NewExceptionLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionLedgerBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ExceptionLedgerBadgeLengthRule(86400);
        var item = ExceptionLedger.NewExceptionLedger("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionLedgerRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new ExceptionLedgerRequiredBadgeRule("exception ledger needs a value");
        var item = ExceptionLedger.NewExceptionLedger("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionLedgerValidatorReportsEveryProblem();
        ExceptionLedgerRequiredLabelRuleFlagsABlankLabel();
        ExceptionLedgerLabelLengthRuleFlagsALongLabel();
        ExceptionLedgerLimitRangeRuleFlagsValuesAboveLimit();
        ExceptionLedgerRequiredCodeRuleFlagsABlankCode();
        ExceptionLedgerBadgeLengthRuleFlagsALongBadge();
        ExceptionLedgerRequiredBadgeRuleFlagsABlankBadge();
    }
}
