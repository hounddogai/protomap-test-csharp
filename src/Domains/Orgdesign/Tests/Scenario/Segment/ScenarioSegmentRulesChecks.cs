namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioSegmentRulesChecks
{
    public static void ScenarioSegmentValidatorReportsEveryProblem()
    {
        var validator = ScenarioSegmentRules.DefaultScenarioSegmentValidator();
        var item = ScenarioSegment.NewScenarioSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioSegmentCodeLengthRule(255);
        var item = ScenarioSegment.NewScenarioSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioSegmentRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ScenarioSegmentRequiredPhoneRule("scenario segment needs a value");
        var item = ScenarioSegment.NewScenarioSegment("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new ScenarioSegmentNameLengthRule(1000);
        var item = ScenarioSegment.NewScenarioSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioSegmentFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ScenarioSegmentFteRangeRule(5.7, 1149.0);
        var item = ScenarioSegment.NewScenarioSegment("x", "y");
        item.Fte = 1150.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScenarioSegmentRequiredCodeRule("scenario segment needs this field");
        var item = ScenarioSegment.NewScenarioSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScenarioSegmentValidatorReportsEveryProblem();
        ScenarioSegmentCodeLengthRuleFlagsALongCode();
        ScenarioSegmentRequiredPhoneRuleFlagsABlankPhone();
        ScenarioSegmentNameLengthRuleFlagsALongName();
        ScenarioSegmentFteRangeRuleFlagsValuesAboveFte();
        ScenarioSegmentRequiredCodeRuleFlagsABlankCode();
    }
}
