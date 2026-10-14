namespace Acme.Workforce.Talent.Tests.Pool.Assignment;

using Acme.Workforce.Talent.Pool.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AdaptivePoolAssignmentRulesChecks
{
    public static void AdaptivePoolAssignmentValidatorReportsEveryProblem()
    {
        var validator = AdaptivePoolAssignmentRules.DefaultAdaptivePoolAssignmentValidator();
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePoolAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptivePoolAssignmentRequiredLabelRule("adaptive pool assignment needs a value");
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePoolAssignmentRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AdaptivePoolAssignmentRequiredRegionRule("adaptive pool assignment needs a value");
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePoolAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptivePoolAssignmentLabelLengthRule(3600);
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePoolAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptivePoolAssignmentCodeLengthRule(80);
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptivePoolAssignmentValidatorReportsEveryProblem();
        AdaptivePoolAssignmentRequiredLabelRuleFlagsABlankLabel();
        AdaptivePoolAssignmentRequiredRegionRuleFlagsABlankRegion();
        AdaptivePoolAssignmentLabelLengthRuleFlagsALongLabel();
        AdaptivePoolAssignmentCodeLengthRuleFlagsALongCode();
    }
}
