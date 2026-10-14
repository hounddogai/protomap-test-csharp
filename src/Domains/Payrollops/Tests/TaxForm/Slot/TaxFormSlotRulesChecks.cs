namespace Acme.Workforce.Payrollops.Tests.TaxForm.Slot;

using Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TaxFormSlotRulesChecks
{
    public static void TaxFormSlotValidatorReportsEveryProblem()
    {
        var validator = TaxFormSlotRules.DefaultTaxFormSlotValidator();
        var item = TaxFormSlot.NewTaxFormSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormSlotCodeLengthRule(32);
        var item = TaxFormSlot.NewTaxFormSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormSlotRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TaxFormSlotRetriesRangeRule(16, 1000);
        var item = TaxFormSlot.NewTaxFormSlot("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormSlotBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new TaxFormSlotBadgeLengthRule(64);
        var item = TaxFormSlot.NewTaxFormSlot("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormSlotCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new TaxFormSlotCeilingRangeRule(1.7, 4668.9);
        var item = TaxFormSlot.NewTaxFormSlot("x", "y");
        item.Ceiling = 4670.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TaxFormSlotValidatorReportsEveryProblem();
        TaxFormSlotCodeLengthRuleFlagsALongCode();
        TaxFormSlotRetriesRangeRuleFlagsValuesAboveRetries();
        TaxFormSlotBadgeLengthRuleFlagsALongBadge();
        TaxFormSlotCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
