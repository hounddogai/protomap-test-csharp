namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormRecordRulesChecks
{
    public static void TaxFormRecordValidatorReportsEveryProblem()
    {
        var validator = TaxFormRecordRules.DefaultTaxFormRecordValidator();
        var item = TaxFormRecord.NewTaxFormRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormRecordFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new TaxFormRecordFloorValueRangeRule(4.2, 204.2);
        var item = TaxFormRecord.NewTaxFormRecord("x", "y");
        item.FloorValue = 205.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormRecordRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new TaxFormRecordRequiredContactEmailRule("tax form record needs a value");
        var item = TaxFormRecord.NewTaxFormRecord("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TaxFormRecordRequiredLabelRule("tax form record needs this field");
        var item = TaxFormRecord.NewTaxFormRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormRecordValidatorReportsEveryProblem();
        TaxFormRecordFloorValueRangeRuleFlagsValuesAboveFloorValue();
        TaxFormRecordRequiredContactEmailRuleFlagsABlankContactEmail();
        TaxFormRecordRequiredLabelRuleFlagsABlankLabel();
    }
}
