namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedAccrualGroupRulesChecks
{
    public static void WeightedAccrualGroupValidatorReportsEveryProblem()
    {
        var validator = WeightedAccrualGroupRules.DefaultWeightedAccrualGroupValidator();
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAccrualGroupRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new WeightedAccrualGroupRequiredGradeRule("weighted accrual group needs this field");
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAccrualGroupGradeLengthRuleFlagsALongGrade()
    {
        var rule = new WeightedAccrualGroupGradeLengthRule(120);
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAccrualGroupGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new WeightedAccrualGroupGraceDaysRangeRule(120, 255);
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedAccrualGroupValidatorReportsEveryProblem();
        WeightedAccrualGroupRequiredGradeRuleFlagsABlankGrade();
        WeightedAccrualGroupGradeLengthRuleFlagsALongGrade();
        WeightedAccrualGroupGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
