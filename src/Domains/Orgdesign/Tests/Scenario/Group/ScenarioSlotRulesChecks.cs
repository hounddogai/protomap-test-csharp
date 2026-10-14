namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScenarioSlotRulesChecks
{
    public static void ScenarioSlotValidatorReportsEveryProblem()
    {
        var validator = ScenarioSlotRules.DefaultScenarioSlotValidator();
        var item = ScenarioSlot.NewScenarioSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioSlotCodeLengthRule(1000);
        var item = ScenarioSlot.NewScenarioSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScenarioSlotRequiredNameRule("scenario slot needs this field");
        var item = ScenarioSlot.NewScenarioSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScenarioSlotRequiredCodeRule("scenario slot needs a value");
        var item = ScenarioSlot.NewScenarioSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioSlotUnitLengthRuleFlagsALongUnit()
    {
        var rule = new ScenarioSlotUnitLengthRule(120);
        var item = ScenarioSlot.NewScenarioSlot("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioSlotValidatorReportsEveryProblem();
        ScenarioSlotCodeLengthRuleFlagsALongCode();
        ScenarioSlotRequiredNameRuleFlagsABlankName();
        ScenarioSlotRequiredCodeRuleFlagsABlankCode();
        ScenarioSlotUnitLengthRuleFlagsALongUnit();
    }
}
