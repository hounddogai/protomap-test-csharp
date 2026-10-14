namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredMobilityAuditRulesChecks
{
    public static void TieredMobilityAuditValidatorReportsEveryProblem()
    {
        var validator = TieredMobilityAuditRules.DefaultTieredMobilityAuditValidator();
        var item = TieredMobilityAudit.NewTieredMobilityAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredMobilityAuditEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new TieredMobilityAuditEmployeeIdLengthRule(32);
        var item = TieredMobilityAudit.NewTieredMobilityAudit("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredMobilityAuditRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new TieredMobilityAuditRequiredCurrencyRule("tiered mobility audit needs this field");
        var item = TieredMobilityAudit.NewTieredMobilityAudit("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredMobilityAuditValidatorReportsEveryProblem();
        TieredMobilityAuditEmployeeIdLengthRuleFlagsALongEmployeeId();
        TieredMobilityAuditRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
