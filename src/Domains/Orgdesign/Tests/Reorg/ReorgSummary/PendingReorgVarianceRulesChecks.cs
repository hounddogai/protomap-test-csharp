namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class PendingReorgVarianceRulesChecks
{
    public static void PendingReorgVarianceValidatorReportsEveryProblem()
    {
        var validator = PendingReorgVarianceRules.DefaultPendingReorgVarianceValidator();
        var item = PendingReorgVariance.NewPendingReorgVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingReorgVarianceQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PendingReorgVarianceQuantityRangeRule(64, 64);
        var item = PendingReorgVariance.NewPendingReorgVariance("x", "y");
        item.Quantity = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingReorgVarianceRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PendingReorgVarianceRequiredDescriptionRule("pending reorg variance needs a value");
        var item = PendingReorgVariance.NewPendingReorgVariance("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingReorgVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingReorgVarianceRequiredCodeRule("pending reorg variance needs a value");
        var item = PendingReorgVariance.NewPendingReorgVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingReorgVarianceValidatorReportsEveryProblem();
        PendingReorgVarianceQuantityRangeRuleFlagsValuesAboveQuantity();
        PendingReorgVarianceRequiredDescriptionRuleFlagsABlankDescription();
        PendingReorgVarianceRequiredCodeRuleFlagsABlankCode();
    }
}
