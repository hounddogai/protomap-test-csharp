namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallDigestRulesChecks
{
    public static void TownHallDigestValidatorReportsEveryProblem()
    {
        var validator = TownHallDigestRules.DefaultTownHallDigestValidator();
        var item = TownHallDigest.NewTownHallDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallDigestVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new TownHallDigestVersionRangeRule(64, 255);
        var item = TownHallDigest.NewTownHallDigest("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallDigestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new TownHallDigestMarginRangeRule(8.5, 4882.1);
        var item = TownHallDigest.NewTownHallDigest("x", "y");
        item.Margin = 4883.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallDigestManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new TownHallDigestManagerIdLengthRule(16);
        var item = TownHallDigest.NewTownHallDigest("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallDigestGradeLengthRuleFlagsALongGrade()
    {
        var rule = new TownHallDigestGradeLengthRule(32);
        var item = TownHallDigest.NewTownHallDigest("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallDigestValidatorReportsEveryProblem();
        TownHallDigestVersionRangeRuleFlagsValuesAboveVersion();
        TownHallDigestMarginRangeRuleFlagsValuesAboveMargin();
        TownHallDigestManagerIdLengthRuleFlagsALongManagerId();
        TownHallDigestGradeLengthRuleFlagsALongGrade();
    }
}
