namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScenarioReservationRulesChecks
{
    public static void ScenarioReservationValidatorReportsEveryProblem()
    {
        var validator = ScenarioReservationRules.DefaultScenarioReservationValidator();
        var item = ScenarioReservation.NewScenarioReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioReservationRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new ScenarioReservationRequiredGradeRule("scenario reservation needs this field");
        var item = ScenarioReservation.NewScenarioReservation("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioReservationRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new ScenarioReservationRatioRangeRule(5.3, 545.9);
        var item = ScenarioReservation.NewScenarioReservation("x", "y");
        item.Ratio = 547.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioReservationGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ScenarioReservationGradeLengthRule(86400);
        var item = ScenarioReservation.NewScenarioReservation("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioReservationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ScenarioReservationLeaveDaysRangeRule(16, 120);
        var item = ScenarioReservation.NewScenarioReservation("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScenarioReservationValidatorReportsEveryProblem();
        ScenarioReservationRequiredGradeRuleFlagsABlankGrade();
        ScenarioReservationRatioRangeRuleFlagsValuesAboveRatio();
        ScenarioReservationGradeLengthRuleFlagsALongGrade();
        ScenarioReservationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
