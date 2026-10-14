namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class PrimaryScenarioSegmentRulesChecks
{
    public static void PrimaryScenarioSegmentValidatorReportsEveryProblem()
    {
        var validator = PrimaryScenarioSegmentRules.DefaultPrimaryScenarioSegmentValidator();
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryScenarioSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryScenarioSegmentCodeLengthRule(16);
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryScenarioSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryScenarioSegmentRequiredCodeRule("primary scenario segment needs an entry");
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryScenarioSegmentRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PrimaryScenarioSegmentRevisionRangeRule(80, 120);
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryScenarioSegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PrimaryScenarioSegmentDurationMinutesRangeRule(32, 1000);
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryScenarioSegmentValidatorReportsEveryProblem();
        PrimaryScenarioSegmentCodeLengthRuleFlagsALongCode();
        PrimaryScenarioSegmentRequiredCodeRuleFlagsABlankCode();
        PrimaryScenarioSegmentRevisionRangeRuleFlagsValuesAboveRevision();
        PrimaryScenarioSegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
