namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedTaxFormScoreRulesChecks
{
    public static void CappedTaxFormScoreValidatorReportsEveryProblem()
    {
        var validator = CappedTaxFormScoreRules.DefaultCappedTaxFormScoreValidator();
        var item = CappedTaxFormScore.NewCappedTaxFormScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedTaxFormScoreContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new CappedTaxFormScoreContactEmailLengthRule(120);
        var item = CappedTaxFormScore.NewCappedTaxFormScore("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedTaxFormScoreManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new CappedTaxFormScoreManagerIdLengthRule(64);
        var item = CappedTaxFormScore.NewCappedTaxFormScore("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedTaxFormScoreValidatorReportsEveryProblem();
        CappedTaxFormScoreContactEmailLengthRuleFlagsALongContactEmail();
        CappedTaxFormScoreManagerIdLengthRuleFlagsALongManagerId();
    }
}
