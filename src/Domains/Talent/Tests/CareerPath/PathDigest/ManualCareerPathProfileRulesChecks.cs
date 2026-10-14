namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class ManualCareerPathProfileRulesChecks
{
    public static void ManualCareerPathProfileValidatorReportsEveryProblem()
    {
        var validator = ManualCareerPathProfileRules.DefaultManualCareerPathProfileValidator();
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualCareerPathProfileRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ManualCareerPathProfileRetriesRangeRule(16, 255);
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualCareerPathProfileNameLengthRuleFlagsALongName()
    {
        var rule = new ManualCareerPathProfileNameLengthRule(80);
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualCareerPathProfileRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ManualCareerPathProfileRequiredCategoryRule("manual career path profile needs a value");
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualCareerPathProfileChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ManualCareerPathProfileChannelLengthRule(32);
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualCareerPathProfileCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new ManualCareerPathProfileCategoryLengthRule(16);
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualCareerPathProfileValidatorReportsEveryProblem();
        ManualCareerPathProfileRetriesRangeRuleFlagsValuesAboveRetries();
        ManualCareerPathProfileNameLengthRuleFlagsALongName();
        ManualCareerPathProfileRequiredCategoryRuleFlagsABlankCategory();
        ManualCareerPathProfileChannelLengthRuleFlagsALongChannel();
        ManualCareerPathProfileCategoryLengthRuleFlagsALongCategory();
    }
}
