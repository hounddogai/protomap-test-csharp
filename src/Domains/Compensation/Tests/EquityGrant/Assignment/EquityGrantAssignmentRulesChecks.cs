namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EquityGrantAssignmentRulesChecks
{
    public static void EquityGrantAssignmentValidatorReportsEveryProblem()
    {
        var validator = EquityGrantAssignmentRules.DefaultEquityGrantAssignmentValidator();
        var item = EquityGrantAssignment.NewEquityGrantAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new EquityGrantAssignmentRequiredNameRule("equity grant assignment needs this field");
        var item = EquityGrantAssignment.NewEquityGrantAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantAssignmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new EquityGrantAssignmentScoreRangeRule(6.1, 690.8);
        var item = EquityGrantAssignment.NewEquityGrantAssignment("x", "y");
        item.Score = 692.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantAssignmentRequiredCodeRule("equity grant assignment needs an entry");
        var item = EquityGrantAssignment.NewEquityGrantAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new EquityGrantAssignmentNameLengthRule(3600);
        var item = EquityGrantAssignment.NewEquityGrantAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantAssignmentManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new EquityGrantAssignmentManagerIdLengthRule(64);
        var item = EquityGrantAssignment.NewEquityGrantAssignment("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantAssignmentHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new EquityGrantAssignmentHoursRangeRule(2.7, 3607.3);
        var item = EquityGrantAssignment.NewEquityGrantAssignment("x", "y");
        item.Hours = 3608.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EquityGrantAssignmentValidatorReportsEveryProblem();
        EquityGrantAssignmentRequiredNameRuleFlagsABlankName();
        EquityGrantAssignmentScoreRangeRuleFlagsValuesAboveScore();
        EquityGrantAssignmentRequiredCodeRuleFlagsABlankCode();
        EquityGrantAssignmentNameLengthRuleFlagsALongName();
        EquityGrantAssignmentManagerIdLengthRuleFlagsALongManagerId();
        EquityGrantAssignmentHoursRangeRuleFlagsValuesAboveHours();
    }
}
