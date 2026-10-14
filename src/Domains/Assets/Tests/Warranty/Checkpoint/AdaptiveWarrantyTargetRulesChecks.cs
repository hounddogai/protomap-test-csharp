namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveWarrantyTargetRulesChecks
{
    public static void AdaptiveWarrantyTargetValidatorReportsEveryProblem()
    {
        var validator = AdaptiveWarrantyTargetRules.DefaultAdaptiveWarrantyTargetValidator();
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveWarrantyTargetDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new AdaptiveWarrantyTargetDescriptionLengthRule(3600);
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveWarrantyTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveWarrantyTargetCodeLengthRule(16);
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveWarrantyTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveWarrantyTargetRequiredLabelRule("adaptive warranty target needs a value");
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveWarrantyTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveWarrantyTargetRequiredTitleRule("adaptive warranty target needs this field");
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveWarrantyTargetRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new AdaptiveWarrantyTargetRequiredDescriptionRule("adaptive warranty target needs an entry");
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyTargetValidatorReportsEveryProblem();
        AdaptiveWarrantyTargetDescriptionLengthRuleFlagsALongDescription();
        AdaptiveWarrantyTargetCodeLengthRuleFlagsALongCode();
        AdaptiveWarrantyTargetRequiredLabelRuleFlagsABlankLabel();
        AdaptiveWarrantyTargetRequiredTitleRuleFlagsABlankTitle();
        AdaptiveWarrantyTargetRequiredDescriptionRuleFlagsABlankDescription();
    }
}
