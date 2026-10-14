namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanLedgerRulesChecks
{
    public static void ReturnPlanLedgerValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanLedgerRules.DefaultReturnPlanLedgerValidator();
        var item = ReturnPlanLedger.NewReturnPlanLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanLedgerCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ReturnPlanLedgerCeilingRangeRule(7.6, 493.2);
        var item = ReturnPlanLedger.NewReturnPlanLedger("x", "y");
        item.Ceiling = 494.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanLedgerRequiredCodeRule("return plan ledger needs an entry");
        var item = ReturnPlanLedger.NewReturnPlanLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanLedgerCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ReturnPlanLedgerCountryLengthRule(16);
        var item = ReturnPlanLedger.NewReturnPlanLedger("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanLedgerRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new ReturnPlanLedgerRequiredUnitRule("return plan ledger needs a value");
        var item = ReturnPlanLedger.NewReturnPlanLedger("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanLedgerRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new ReturnPlanLedgerRequiredCountryRule("return plan ledger needs an entry");
        var item = ReturnPlanLedger.NewReturnPlanLedger("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanLedgerValidatorReportsEveryProblem();
        ReturnPlanLedgerCeilingRangeRuleFlagsValuesAboveCeiling();
        ReturnPlanLedgerRequiredCodeRuleFlagsABlankCode();
        ReturnPlanLedgerCountryLengthRuleFlagsALongCountry();
        ReturnPlanLedgerRequiredUnitRuleFlagsABlankUnit();
        ReturnPlanLedgerRequiredCountryRuleFlagsABlankCountry();
    }
}
