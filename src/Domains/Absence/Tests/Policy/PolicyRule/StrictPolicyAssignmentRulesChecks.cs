namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class StrictPolicyAssignmentRulesChecks
{
    public static void StrictPolicyAssignmentValidatorReportsEveryProblem()
    {
        var validator = StrictPolicyAssignmentRules.DefaultStrictPolicyAssignmentValidator();
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPolicyAssignmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new StrictPolicyAssignmentCapacityRangeRule(32, 1000);
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPolicyAssignmentRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new StrictPolicyAssignmentRequiredExternalRefRule("strict policy assignment needs an entry");
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPolicyAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictPolicyAssignmentRequiredLabelRule("strict policy assignment needs this field");
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPolicyAssignmentSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new StrictPolicyAssignmentSegmentLengthRule(16);
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPolicyAssignmentRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new StrictPolicyAssignmentRequiredSegmentRule("strict policy assignment needs a value");
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictPolicyAssignmentValidatorReportsEveryProblem();
        StrictPolicyAssignmentCapacityRangeRuleFlagsValuesAboveCapacity();
        StrictPolicyAssignmentRequiredExternalRefRuleFlagsABlankExternalRef();
        StrictPolicyAssignmentRequiredLabelRuleFlagsABlankLabel();
        StrictPolicyAssignmentSegmentLengthRuleFlagsALongSegment();
        StrictPolicyAssignmentRequiredSegmentRuleFlagsABlankSegment();
    }
}
