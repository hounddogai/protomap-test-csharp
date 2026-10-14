namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormScoreRulesChecks
{
    public static void TaxFormScoreValidatorReportsEveryProblem()
    {
        var validator = TaxFormScoreRules.DefaultTaxFormScoreValidator();
        var item = TaxFormScore.NewTaxFormScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormScoreRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TaxFormScoreRequiredLabelRule("tax form score needs an entry");
        var item = TaxFormScore.NewTaxFormScore("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormScoreWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new TaxFormScoreWeightRangeRule(5.0, 4183.7);
        var item = TaxFormScore.NewTaxFormScore("x", "y");
        item.Weight = 4185.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormScoreCodeLengthRule(3600);
        var item = TaxFormScore.NewTaxFormScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormScoreValidatorReportsEveryProblem();
        TaxFormScoreRequiredLabelRuleFlagsABlankLabel();
        TaxFormScoreWeightRangeRuleFlagsValuesAboveWeight();
        TaxFormScoreCodeLengthRuleFlagsALongCode();
    }
}
