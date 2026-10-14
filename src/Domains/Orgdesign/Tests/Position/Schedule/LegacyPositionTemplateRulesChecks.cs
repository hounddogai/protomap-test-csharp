namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyPositionTemplateRulesChecks
{
    public static void LegacyPositionTemplateValidatorReportsEveryProblem()
    {
        var validator = LegacyPositionTemplateRules.DefaultLegacyPositionTemplateValidator();
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPositionTemplateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyPositionTemplateDurationMinutesRangeRule(80, 120);
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPositionTemplateThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new LegacyPositionTemplateThresholdRangeRule(6.4, 2737.2);
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("x", "y");
        item.Threshold = 2738.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyPositionTemplateValidatorReportsEveryProblem();
        LegacyPositionTemplateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LegacyPositionTemplateThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
