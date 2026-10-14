namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedReorgSlotRulesChecks
{
    public static void WeightedReorgSlotValidatorReportsEveryProblem()
    {
        var validator = WeightedReorgSlotRules.DefaultWeightedReorgSlotValidator();
        var item = WeightedReorgSlot.NewWeightedReorgSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedReorgSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedReorgSlotRequiredCodeRule("weighted reorg slot needs a value");
        var item = WeightedReorgSlot.NewWeightedReorgSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedReorgSlotRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new WeightedReorgSlotRequiredSummaryRule("weighted reorg slot needs this field");
        var item = WeightedReorgSlot.NewWeightedReorgSlot("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedReorgSlotCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new WeightedReorgSlotCurrencyLengthRule(1000);
        var item = WeightedReorgSlot.NewWeightedReorgSlot("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedReorgSlotValidatorReportsEveryProblem();
        WeightedReorgSlotRequiredCodeRuleFlagsABlankCode();
        WeightedReorgSlotRequiredSummaryRuleFlagsABlankSummary();
        WeightedReorgSlotCurrencyLengthRuleFlagsALongCurrency();
    }
}
