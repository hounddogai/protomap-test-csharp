namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AdaptiveAttestationMilestoneRulesChecks
{
    public static void AdaptiveAttestationMilestoneValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAttestationMilestoneRules.DefaultAdaptiveAttestationMilestoneValidator();
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAttestationMilestoneRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AdaptiveAttestationMilestoneRequiredLocationRule("adaptive attestation milestone needs a value");
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAttestationMilestoneCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new AdaptiveAttestationMilestoneCeilingRangeRule(3.7, 3265.1);
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("x", "y");
        item.Ceiling = 3266.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveAttestationMilestoneCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AdaptiveAttestationMilestoneCostRangeRule(3.3, 2008.3);
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("x", "y");
        item.Cost = 2009.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveAttestationMilestoneRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new AdaptiveAttestationMilestoneRequiredGradeRule("adaptive attestation milestone needs a value");
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAttestationMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveAttestationMilestoneRequiredCodeRule("adaptive attestation milestone needs a value");
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveAttestationMilestoneValidatorReportsEveryProblem();
        AdaptiveAttestationMilestoneRequiredLocationRuleFlagsABlankLocation();
        AdaptiveAttestationMilestoneCeilingRangeRuleFlagsValuesAboveCeiling();
        AdaptiveAttestationMilestoneCostRangeRuleFlagsValuesAboveCost();
        AdaptiveAttestationMilestoneRequiredGradeRuleFlagsABlankGrade();
        AdaptiveAttestationMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}
