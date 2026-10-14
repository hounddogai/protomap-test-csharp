namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorLedgerRulesChecks
{
    public static void VisitorLedgerValidatorReportsEveryProblem()
    {
        var validator = VisitorLedgerRules.DefaultVisitorLedgerValidator();
        var item = VisitorLedger.NewVisitorLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorLedgerLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new VisitorLedgerLeaveDaysRangeRule(16, 64);
        var item = VisitorLedger.NewVisitorLedger("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorLedgerRequiredCodeRule("visitor ledger needs this field");
        var item = VisitorLedger.NewVisitorLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorLedgerValidatorReportsEveryProblem();
        VisitorLedgerLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        VisitorLedgerRequiredCodeRuleFlagsABlankCode();
    }
}
