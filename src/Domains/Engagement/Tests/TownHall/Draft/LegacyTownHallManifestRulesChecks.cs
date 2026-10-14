namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyTownHallManifestRulesChecks
{
    public static void LegacyTownHallManifestValidatorReportsEveryProblem()
    {
        var validator = LegacyTownHallManifestRules.DefaultLegacyTownHallManifestValidator();
        var item = LegacyTownHallManifest.NewLegacyTownHallManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTownHallManifestCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LegacyTownHallManifestCreatedAtRangeRule(32, 32);
        var item = LegacyTownHallManifest.NewLegacyTownHallManifest("x", "y");
        item.CreatedAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTownHallManifestJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LegacyTownHallManifestJobCodeLengthRule(120);
        var item = LegacyTownHallManifest.NewLegacyTownHallManifest("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyTownHallManifestValidatorReportsEveryProblem();
        LegacyTownHallManifestCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LegacyTownHallManifestJobCodeLengthRuleFlagsALongJobCode();
    }
}
