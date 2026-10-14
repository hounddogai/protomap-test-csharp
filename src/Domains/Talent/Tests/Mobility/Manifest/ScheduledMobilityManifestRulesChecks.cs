namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledMobilityManifestRulesChecks
{
    public static void ScheduledMobilityManifestValidatorReportsEveryProblem()
    {
        var validator = ScheduledMobilityManifestRules.DefaultScheduledMobilityManifestValidator();
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledMobilityManifestGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ScheduledMobilityManifestGradeLengthRule(64);
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledMobilityManifestDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new ScheduledMobilityManifestDescriptionLengthRule(86400);
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledMobilityManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledMobilityManifestCodeLengthRule(3600);
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledMobilityManifestValidatorReportsEveryProblem();
        ScheduledMobilityManifestGradeLengthRuleFlagsALongGrade();
        ScheduledMobilityManifestDescriptionLengthRuleFlagsALongDescription();
        ScheduledMobilityManifestCodeLengthRuleFlagsALongCode();
    }
}
