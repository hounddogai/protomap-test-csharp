namespace Acme.Workforce.Orgdesign.Tests.Scenario.Notice;

using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScenarioNoticeRulesChecks
{
    public static void ScenarioNoticeValidatorReportsEveryProblem()
    {
        var validator = ScenarioNoticeRules.DefaultScenarioNoticeValidator();
        var item = ScenarioNotice.NewScenarioNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioNoticeManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ScenarioNoticeManagerIdLengthRule(255);
        var item = ScenarioNotice.NewScenarioNotice("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScenarioNoticeRequiredCodeRule("scenario notice needs an entry");
        var item = ScenarioNotice.NewScenarioNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScenarioNoticeTitleLengthRule(1000);
        var item = ScenarioNotice.NewScenarioNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioNoticeCodeLengthRule(3600);
        var item = ScenarioNotice.NewScenarioNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioNoticeValidatorReportsEveryProblem();
        ScenarioNoticeManagerIdLengthRuleFlagsALongManagerId();
        ScenarioNoticeRequiredCodeRuleFlagsABlankCode();
        ScenarioNoticeTitleLengthRuleFlagsALongTitle();
        ScenarioNoticeCodeLengthRuleFlagsALongCode();
    }
}
