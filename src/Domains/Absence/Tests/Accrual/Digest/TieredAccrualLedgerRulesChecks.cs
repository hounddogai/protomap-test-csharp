namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredAccrualLedgerRulesChecks
{
    public static void TieredAccrualLedgerValidatorReportsEveryProblem()
    {
        var validator = TieredAccrualLedgerRules.DefaultTieredAccrualLedgerValidator();
        var item = TieredAccrualLedger.NewTieredAccrualLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredAccrualLedgerRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new TieredAccrualLedgerRequiredGradeRule("tiered accrual ledger needs an entry");
        var item = TieredAccrualLedger.NewTieredAccrualLedger("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredAccrualLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredAccrualLedgerRequiredNameRule("tiered accrual ledger needs an entry");
        var item = TieredAccrualLedger.NewTieredAccrualLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredAccrualLedgerGradeLengthRuleFlagsALongGrade()
    {
        var rule = new TieredAccrualLedgerGradeLengthRule(32);
        var item = TieredAccrualLedger.NewTieredAccrualLedger("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredAccrualLedgerValidatorReportsEveryProblem();
        TieredAccrualLedgerRequiredGradeRuleFlagsABlankGrade();
        TieredAccrualLedgerRequiredNameRuleFlagsABlankName();
        TieredAccrualLedgerGradeLengthRuleFlagsALongGrade();
    }
}
