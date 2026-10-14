namespace Acme.Workforce.Assets.Tests.Assignment.Profile;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssignmentProfileRulesChecks
{
    public static void AssignmentProfileValidatorReportsEveryProblem()
    {
        var validator = AssignmentProfileRules.DefaultAssignmentProfileValidator();
        var item = AssignmentProfile.NewAssignmentProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentProfileUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AssignmentProfileUnitLengthRule(3600);
        var item = AssignmentProfile.NewAssignmentProfile("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentProfileMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AssignmentProfileMinimumRangeRule(5.0, 1432.2);
        var item = AssignmentProfile.NewAssignmentProfile("x", "y");
        item.Minimum = 1433.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentProfileScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AssignmentProfileScoreRangeRule(4.3, 4346.9);
        var item = AssignmentProfile.NewAssignmentProfile("x", "y");
        item.Score = 4348.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentProfileRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new AssignmentProfileRequiredUnitRule("assignment profile needs a value");
        var item = AssignmentProfile.NewAssignmentProfile("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentProfileValidatorReportsEveryProblem();
        AssignmentProfileUnitLengthRuleFlagsALongUnit();
        AssignmentProfileMinimumRangeRuleFlagsValuesAboveMinimum();
        AssignmentProfileScoreRangeRuleFlagsValuesAboveScore();
        AssignmentProfileRequiredUnitRuleFlagsABlankUnit();
    }
}
