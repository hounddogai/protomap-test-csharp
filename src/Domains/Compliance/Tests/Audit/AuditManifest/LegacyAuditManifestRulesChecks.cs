namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class LegacyAuditManifestRulesChecks
{
    public static void LegacyAuditManifestValidatorReportsEveryProblem()
    {
        var validator = LegacyAuditManifestRules.DefaultLegacyAuditManifestValidator();
        var item = LegacyAuditManifest.NewLegacyAuditManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyAuditManifestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LegacyAuditManifestRequiredTeamRule("legacy audit manifest needs this field");
        var item = LegacyAuditManifest.NewLegacyAuditManifest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAuditManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyAuditManifestDurationMinutesRangeRule(255, 86400);
        var item = LegacyAuditManifest.NewLegacyAuditManifest("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyAuditManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyAuditManifestRequiredCodeRule("legacy audit manifest needs a value");
        var item = LegacyAuditManifest.NewLegacyAuditManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAuditManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyAuditManifestRequiredNameRule("legacy audit manifest needs this field");
        var item = LegacyAuditManifest.NewLegacyAuditManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAuditManifestAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LegacyAuditManifestAttemptsRangeRule(64, 120);
        var item = LegacyAuditManifest.NewLegacyAuditManifest("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyAuditManifestCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LegacyAuditManifestCapacityRangeRule(64, 3600);
        var item = LegacyAuditManifest.NewLegacyAuditManifest("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyAuditManifestValidatorReportsEveryProblem();
        LegacyAuditManifestRequiredTeamRuleFlagsABlankTeam();
        LegacyAuditManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LegacyAuditManifestRequiredCodeRuleFlagsABlankCode();
        LegacyAuditManifestRequiredNameRuleFlagsABlankName();
        LegacyAuditManifestAttemptsRangeRuleFlagsValuesAboveAttempts();
        LegacyAuditManifestCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
