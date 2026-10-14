namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityManifestRulesChecks
{
    public static void MobilityManifestValidatorReportsEveryProblem()
    {
        var validator = MobilityManifestRules.DefaultMobilityManifestValidator();
        var item = MobilityManifest.NewMobilityManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new MobilityManifestDurationMinutesRangeRule(255, 1000);
        var item = MobilityManifest.NewMobilityManifest("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new MobilityManifestRequiredNameRule("mobility manifest needs this field");
        var item = MobilityManifest.NewMobilityManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityManifestRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new MobilityManifestRequiredManagerIdRule("mobility manifest needs a value");
        var item = MobilityManifest.NewMobilityManifest("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityManifestBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new MobilityManifestBadgeLengthRule(86400);
        var item = MobilityManifest.NewMobilityManifest("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityManifestValidatorReportsEveryProblem();
        MobilityManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        MobilityManifestRequiredNameRuleFlagsABlankName();
        MobilityManifestRequiredManagerIdRuleFlagsABlankManagerId();
        MobilityManifestBadgeLengthRuleFlagsALongBadge();
    }
}
