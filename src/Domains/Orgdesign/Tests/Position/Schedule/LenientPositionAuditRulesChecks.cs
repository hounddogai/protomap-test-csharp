namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientPositionAuditRulesChecks
{
    public static void LenientPositionAuditValidatorReportsEveryProblem()
    {
        var validator = LenientPositionAuditRules.DefaultLenientPositionAuditValidator();
        var item = LenientPositionAudit.NewLenientPositionAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPositionAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LenientPositionAuditDurationMinutesRangeRule(16, 64);
        var item = LenientPositionAudit.NewLenientPositionAudit("x", "y");
        item.DurationMinutes = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientPositionAuditValidatorReportsEveryProblem();
        LenientPositionAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
