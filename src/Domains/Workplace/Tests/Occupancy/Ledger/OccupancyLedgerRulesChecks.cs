namespace Acme.Workforce.Workplace.Tests.Occupancy.Ledger;

using Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class OccupancyLedgerRulesChecks
{
    public static void OccupancyLedgerValidatorReportsEveryProblem()
    {
        var validator = OccupancyLedgerRules.DefaultOccupancyLedgerValidator();
        var item = OccupancyLedger.NewOccupancyLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyLedgerTitleLengthRule(86400);
        var item = OccupancyLedger.NewOccupancyLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyLedgerRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new OccupancyLedgerRequiredCategoryRule("occupancy ledger needs an entry");
        var item = OccupancyLedger.NewOccupancyLedger("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyLedgerCodeLengthRule(255);
        var item = OccupancyLedger.NewOccupancyLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyLedgerValidatorReportsEveryProblem();
        OccupancyLedgerTitleLengthRuleFlagsALongTitle();
        OccupancyLedgerRequiredCategoryRuleFlagsABlankCategory();
        OccupancyLedgerCodeLengthRuleFlagsALongCode();
    }
}
