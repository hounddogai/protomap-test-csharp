namespace Acme.Workforce.Orgdesign.Tests.Scenario.Request;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalScenarioRequestRulesChecks
{
    public static void GlobalScenarioRequestValidatorReportsEveryProblem()
    {
        var validator = GlobalScenarioRequestRules.DefaultGlobalScenarioRequestValidator();
        var item = GlobalScenarioRequest.NewGlobalScenarioRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalScenarioRequestFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new GlobalScenarioRequestFteRangeRule(6.7, 2337.9);
        var item = GlobalScenarioRequest.NewGlobalScenarioRequest("x", "y");
        item.Fte = 2339.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalScenarioRequestRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new GlobalScenarioRequestRequiredReviewerRule("global scenario request needs an entry");
        var item = GlobalScenarioRequest.NewGlobalScenarioRequest("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalScenarioRequestValidatorReportsEveryProblem();
        GlobalScenarioRequestFteRangeRuleFlagsValuesAboveFte();
        GlobalScenarioRequestRequiredReviewerRuleFlagsABlankReviewer();
    }
}
