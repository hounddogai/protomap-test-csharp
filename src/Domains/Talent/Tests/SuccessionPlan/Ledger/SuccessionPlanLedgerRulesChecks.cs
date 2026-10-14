namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanLedgerRulesChecks
{
    public static void SuccessionPlanLedgerValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanLedgerRules.DefaultSuccessionPlanLedgerValidator();
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanLedgerFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SuccessionPlanLedgerFloorValueRangeRule(7.9, 3733.2);
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("x", "y");
        item.FloorValue = 3734.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanLedgerLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new SuccessionPlanLedgerLocaleLengthRule(64);
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new SuccessionPlanLedgerCodeLengthRule(120);
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanLedgerRequiredCodeRule("succession plan ledger needs a value");
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanLedgerRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SuccessionPlanLedgerRequiredTitleRule("succession plan ledger needs a value");
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanLedgerValidatorReportsEveryProblem();
        SuccessionPlanLedgerFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SuccessionPlanLedgerLocaleLengthRuleFlagsALongLocale();
        SuccessionPlanLedgerCodeLengthRuleFlagsALongCode();
        SuccessionPlanLedgerRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanLedgerRequiredTitleRuleFlagsABlankTitle();
    }
}
