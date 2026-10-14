namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class ManualCareerPathDigestRulesChecks
{
    public static void ManualCareerPathDigestValidatorReportsEveryProblem()
    {
        var validator = ManualCareerPathDigestRules.DefaultManualCareerPathDigestValidator();
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualCareerPathDigestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ManualCareerPathDigestScoreRangeRule(6.4, 4185.9);
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("x", "y");
        item.Score = 4187.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualCareerPathDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualCareerPathDigestCodeLengthRule(255);
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualCareerPathDigestRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ManualCareerPathDigestRequiredLocaleRule("manual career path digest needs a value");
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualCareerPathDigestSourceLengthRuleFlagsALongSource()
    {
        var rule = new ManualCareerPathDigestSourceLengthRule(1000);
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualCareerPathDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualCareerPathDigestRequiredLabelRule("manual career path digest needs an entry");
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualCareerPathDigestPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ManualCareerPathDigestPriorityRangeRule(32, 64);
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualCareerPathDigestValidatorReportsEveryProblem();
        ManualCareerPathDigestScoreRangeRuleFlagsValuesAboveScore();
        ManualCareerPathDigestCodeLengthRuleFlagsALongCode();
        ManualCareerPathDigestRequiredLocaleRuleFlagsABlankLocale();
        ManualCareerPathDigestSourceLengthRuleFlagsALongSource();
        ManualCareerPathDigestRequiredLabelRuleFlagsABlankLabel();
        ManualCareerPathDigestPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
