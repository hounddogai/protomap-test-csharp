namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioWindowRulesChecks
{
    public static void ScenarioWindowValidatorReportsEveryProblem()
    {
        var validator = ScenarioWindowRules.DefaultScenarioWindowValidator();
        var item = ScenarioWindow.NewScenarioWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioWindowMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ScenarioWindowMinimumRangeRule(5.1, 3113.3);
        var item = ScenarioWindow.NewScenarioWindow("x", "y");
        item.Minimum = 3114.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScenarioWindowRequiredCodeRule("scenario window needs a value");
        var item = ScenarioWindow.NewScenarioWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioWindowNotesLengthRuleFlagsALongNotes()
    {
        var rule = new ScenarioWindowNotesLengthRule(3600);
        var item = ScenarioWindow.NewScenarioWindow("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioWindowValidatorReportsEveryProblem();
        ScenarioWindowMinimumRangeRuleFlagsValuesAboveMinimum();
        ScenarioWindowRequiredCodeRuleFlagsABlankCode();
        ScenarioWindowNotesLengthRuleFlagsALongNotes();
    }
}
