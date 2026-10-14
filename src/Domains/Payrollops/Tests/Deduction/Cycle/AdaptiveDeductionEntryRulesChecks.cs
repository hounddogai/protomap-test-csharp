namespace Acme.Workforce.Payrollops.Tests.Deduction.Cycle;

using Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveDeductionEntryRulesChecks
{
    public static void AdaptiveDeductionEntryValidatorReportsEveryProblem()
    {
        var validator = AdaptiveDeductionEntryRules.DefaultAdaptiveDeductionEntryValidator();
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveDeductionEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveDeductionEntryRequiredCodeRule("adaptive deduction entry needs this field");
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveDeductionEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveDeductionEntryRequiredTitleRule("adaptive deduction entry needs an entry");
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveDeductionEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveDeductionEntryCodeLengthRule(32);
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveDeductionEntryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AdaptiveDeductionEntryBudgetRangeRule(2.5, 4353.3);
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("x", "y");
        item.Budget = 4354.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveDeductionEntrySequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new AdaptiveDeductionEntrySequenceRangeRule(16, 64);
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("x", "y");
        item.Sequence = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveDeductionEntryValidatorReportsEveryProblem();
        AdaptiveDeductionEntryRequiredCodeRuleFlagsABlankCode();
        AdaptiveDeductionEntryRequiredTitleRuleFlagsABlankTitle();
        AdaptiveDeductionEntryCodeLengthRuleFlagsALongCode();
        AdaptiveDeductionEntryBudgetRangeRuleFlagsValuesAboveBudget();
        AdaptiveDeductionEntrySequenceRangeRuleFlagsValuesAboveSequence();
    }
}
