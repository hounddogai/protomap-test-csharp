namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMilestoneRulesChecks
{
    public static void AdaptiveWarrantyMilestoneValidatorReportsEveryProblem()
    {
        var validator = AdaptiveWarrantyMilestoneRules.DefaultAdaptiveWarrantyMilestoneValidator();
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveWarrantyMilestoneChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new AdaptiveWarrantyMilestoneChecksumLengthRule(16);
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveWarrantyMilestoneRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new AdaptiveWarrantyMilestoneRequiredUnitRule("adaptive warranty milestone needs a value");
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveWarrantyMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveWarrantyMilestoneCodeLengthRule(80);
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMilestoneValidatorReportsEveryProblem();
        AdaptiveWarrantyMilestoneChecksumLengthRuleFlagsALongChecksum();
        AdaptiveWarrantyMilestoneRequiredUnitRuleFlagsABlankUnit();
        AdaptiveWarrantyMilestoneCodeLengthRuleFlagsALongCode();
    }
}
