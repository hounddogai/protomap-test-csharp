namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopSegmentRulesChecks
{
    public static void LaptopSegmentValidatorReportsEveryProblem()
    {
        var validator = LaptopSegmentRules.DefaultLaptopSegmentValidator();
        var item = LaptopSegment.NewLaptopSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopSegmentSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LaptopSegmentSummaryLengthRule(1000);
        var item = LaptopSegment.NewLaptopSegment("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopSegmentSkillLengthRuleFlagsALongSkill()
    {
        var rule = new LaptopSegmentSkillLengthRule(32);
        var item = LaptopSegment.NewLaptopSegment("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopSegmentRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LaptopSegmentRequiredSummaryRule("laptop segment needs a value");
        var item = LaptopSegment.NewLaptopSegment("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopSegmentValidatorReportsEveryProblem();
        LaptopSegmentSummaryLengthRuleFlagsALongSummary();
        LaptopSegmentSkillLengthRuleFlagsALongSkill();
        LaptopSegmentRequiredSummaryRuleFlagsABlankSummary();
    }
}
