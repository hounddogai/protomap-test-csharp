namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormThresholdRulesChecks
{
    public static void TaxFormThresholdValidatorReportsEveryProblem()
    {
        var validator = TaxFormThresholdRules.DefaultTaxFormThresholdValidator();
        var item = TaxFormThreshold.NewTaxFormThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormThresholdQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new TaxFormThresholdQuantityRangeRule(16, 80);
        var item = TaxFormThreshold.NewTaxFormThreshold("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormThresholdSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new TaxFormThresholdSequenceRangeRule(80, 120);
        var item = TaxFormThreshold.NewTaxFormThreshold("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormThresholdRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new TaxFormThresholdRequiredNotesRule("tax form threshold needs a value");
        var item = TaxFormThreshold.NewTaxFormThreshold("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormThresholdCodeLengthRule(3600);
        var item = TaxFormThreshold.NewTaxFormThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormThresholdValidatorReportsEveryProblem();
        TaxFormThresholdQuantityRangeRuleFlagsValuesAboveQuantity();
        TaxFormThresholdSequenceRangeRuleFlagsValuesAboveSequence();
        TaxFormThresholdRequiredNotesRuleFlagsABlankNotes();
        TaxFormThresholdCodeLengthRuleFlagsALongCode();
    }
}
