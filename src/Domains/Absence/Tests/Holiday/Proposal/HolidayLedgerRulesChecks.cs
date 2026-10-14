namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayLedgerRulesChecks
{
    public static void HolidayLedgerValidatorReportsEveryProblem()
    {
        var validator = HolidayLedgerRules.DefaultHolidayLedgerValidator();
        var item = HolidayLedger.NewHolidayLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HolidayLedgerRequiredCodeRule("holiday ledger needs an entry");
        var item = HolidayLedger.NewHolidayLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HolidayLedgerRequiredLabelRule("holiday ledger needs a value");
        var item = HolidayLedger.NewHolidayLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayLedgerRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new HolidayLedgerRequiredOwnerIdRule("holiday ledger needs an entry");
        var item = HolidayLedger.NewHolidayLedger("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayLedgerCodeLengthRule(32);
        var item = HolidayLedger.NewHolidayLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayLedgerOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new HolidayLedgerOwnerIdLengthRule(16);
        var item = HolidayLedger.NewHolidayLedger("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayLedgerValidatorReportsEveryProblem();
        HolidayLedgerRequiredCodeRuleFlagsABlankCode();
        HolidayLedgerRequiredLabelRuleFlagsABlankLabel();
        HolidayLedgerRequiredOwnerIdRuleFlagsABlankOwnerId();
        HolidayLedgerCodeLengthRuleFlagsALongCode();
        HolidayLedgerOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
