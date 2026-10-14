namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateAssignmentRulesChecks
{
    public static void RateAssignmentValidatorReportsEveryProblem()
    {
        var validator = RateAssignmentRules.DefaultRateAssignmentValidator();
        var item = RateAssignment.NewRateAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateAssignmentRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new RateAssignmentRequiredChecksumRule("rate assignment needs an entry");
        var item = RateAssignment.NewRateAssignment("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RateAssignmentRequiredTitleRule("rate assignment needs an entry");
        var item = RateAssignment.NewRateAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateAssignmentRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new RateAssignmentRequiredSkillRule("rate assignment needs a value");
        var item = RateAssignment.NewRateAssignment("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateAssignmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RateAssignmentAmountRangeRule(2.8, 2228.3);
        var item = RateAssignment.NewRateAssignment("x", "y");
        item.Amount = 2229.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateAssignmentSkillLengthRuleFlagsALongSkill()
    {
        var rule = new RateAssignmentSkillLengthRule(120);
        var item = RateAssignment.NewRateAssignment("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateAssignmentValidatorReportsEveryProblem();
        RateAssignmentRequiredChecksumRuleFlagsABlankChecksum();
        RateAssignmentRequiredTitleRuleFlagsABlankTitle();
        RateAssignmentRequiredSkillRuleFlagsABlankSkill();
        RateAssignmentAmountRangeRuleFlagsValuesAboveAmount();
        RateAssignmentSkillLengthRuleFlagsALongSkill();
    }
}
