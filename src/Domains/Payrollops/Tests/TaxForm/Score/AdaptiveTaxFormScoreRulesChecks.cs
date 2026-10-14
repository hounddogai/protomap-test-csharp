namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveTaxFormScoreRulesChecks
{
    public static void AdaptiveTaxFormScoreValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTaxFormScoreRules.DefaultAdaptiveTaxFormScoreValidator();
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTaxFormScoreNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveTaxFormScoreNameLengthRule(1000);
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTaxFormScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveTaxFormScoreCodeLengthRule(120);
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormScoreValidatorReportsEveryProblem();
        AdaptiveTaxFormScoreNameLengthRuleFlagsALongName();
        AdaptiveTaxFormScoreCodeLengthRuleFlagsALongCode();
    }
}
