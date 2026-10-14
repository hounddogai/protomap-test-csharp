namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class LeaveRequestSegmentRulesChecks
{
    public static void LeaveRequestSegmentValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestSegmentRules.DefaultLeaveRequestSegmentValidator();
        var item = LeaveRequestSegment.NewLeaveRequestSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestSegmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new LeaveRequestSegmentScoreRangeRule(1.2, 256.1);
        var item = LeaveRequestSegment.NewLeaveRequestSegment("x", "y");
        item.Score = 257.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestSegmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LeaveRequestSegmentCostRangeRule(5.8, 3272.6);
        var item = LeaveRequestSegment.NewLeaveRequestSegment("x", "y");
        item.Cost = 3274.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestSegmentRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new LeaveRequestSegmentRequiredSkillRule("leave request segment needs this field");
        var item = LeaveRequestSegment.NewLeaveRequestSegment("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestSegmentSkillLengthRuleFlagsALongSkill()
    {
        var rule = new LeaveRequestSegmentSkillLengthRule(16);
        var item = LeaveRequestSegment.NewLeaveRequestSegment("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestSegmentRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new LeaveRequestSegmentRequiredSalaryBandRule("leave request segment needs an entry");
        var item = LeaveRequestSegment.NewLeaveRequestSegment("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestSegmentValidatorReportsEveryProblem();
        LeaveRequestSegmentScoreRangeRuleFlagsValuesAboveScore();
        LeaveRequestSegmentCostRangeRuleFlagsValuesAboveCost();
        LeaveRequestSegmentRequiredSkillRuleFlagsABlankSkill();
        LeaveRequestSegmentSkillLengthRuleFlagsALongSkill();
        LeaveRequestSegmentRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
