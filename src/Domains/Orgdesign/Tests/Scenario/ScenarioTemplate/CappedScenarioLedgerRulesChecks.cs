namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class CappedScenarioLedgerRulesChecks
{
    public static void CappedScenarioLedgerValidatorReportsEveryProblem()
    {
        var validator = CappedScenarioLedgerRules.DefaultCappedScenarioLedgerValidator();
        var item = CappedScenarioLedger.NewCappedScenarioLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedScenarioLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedScenarioLedgerRequiredCodeRule("capped scenario ledger needs an entry");
        var item = CappedScenarioLedger.NewCappedScenarioLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedScenarioLedgerRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedScenarioLedgerRequiredTitleRule("capped scenario ledger needs a value");
        var item = CappedScenarioLedger.NewCappedScenarioLedger("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedScenarioLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedScenarioLedgerCodeLengthRule(86400);
        var item = CappedScenarioLedger.NewCappedScenarioLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedScenarioLedgerCountryLengthRuleFlagsALongCountry()
    {
        var rule = new CappedScenarioLedgerCountryLengthRule(64);
        var item = CappedScenarioLedger.NewCappedScenarioLedger("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedScenarioLedgerValidatorReportsEveryProblem();
        CappedScenarioLedgerRequiredCodeRuleFlagsABlankCode();
        CappedScenarioLedgerRequiredTitleRuleFlagsABlankTitle();
        CappedScenarioLedgerCodeLengthRuleFlagsALongCode();
        CappedScenarioLedgerCountryLengthRuleFlagsALongCountry();
    }
}
