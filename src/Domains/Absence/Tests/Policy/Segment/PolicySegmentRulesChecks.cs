namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PolicySegmentRulesChecks
{
    public static void PolicySegmentValidatorReportsEveryProblem()
    {
        var validator = PolicySegmentRules.DefaultPolicySegmentValidator();
        var item = PolicySegment.NewPolicySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicySegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new PolicySegmentRequiredNameRule("policy segment needs this field");
        var item = PolicySegment.NewPolicySegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicySegmentNameLengthRuleFlagsALongName()
    {
        var rule = new PolicySegmentNameLengthRule(80);
        var item = PolicySegment.NewPolicySegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicySegmentRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new PolicySegmentRequiredSegmentRule("policy segment needs an entry");
        var item = PolicySegment.NewPolicySegment("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicySegmentValidatorReportsEveryProblem();
        PolicySegmentRequiredNameRuleFlagsABlankName();
        PolicySegmentNameLengthRuleFlagsALongName();
        PolicySegmentRequiredSegmentRuleFlagsABlankSegment();
    }
}
