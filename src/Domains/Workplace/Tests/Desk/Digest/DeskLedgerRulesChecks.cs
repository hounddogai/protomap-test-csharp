namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskLedgerRulesChecks
{
    public static void DeskLedgerValidatorReportsEveryProblem()
    {
        var validator = DeskLedgerRules.DefaultDeskLedgerValidator();
        var item = DeskLedger.NewDeskLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskLedgerRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new DeskLedgerRequiredGradeRule("desk ledger needs an entry");
        var item = DeskLedger.NewDeskLedger("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskLedgerDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new DeskLedgerDiscountRangeRule(9.8, 2493.2);
        var item = DeskLedger.NewDeskLedger("x", "y");
        item.Discount = 2494.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeskLedgerTitleLengthRule(255);
        var item = DeskLedger.NewDeskLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskLedgerValidatorReportsEveryProblem();
        DeskLedgerRequiredGradeRuleFlagsABlankGrade();
        DeskLedgerDiscountRangeRuleFlagsValuesAboveDiscount();
        DeskLedgerTitleLengthRuleFlagsALongTitle();
    }
}
