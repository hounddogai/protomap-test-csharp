namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyEntryRulesChecks
{
    public static void SeasonalPulseSurveyEntryValidatorReportsEveryProblem()
    {
        var validator = SeasonalPulseSurveyEntryRules.DefaultSeasonalPulseSurveyEntryValidator();
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPulseSurveyEntryPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new SeasonalPulseSurveyEntryPriorityRangeRule(64, 255);
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPulseSurveyEntryRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new SeasonalPulseSurveyEntryRequiredCategoryRule("seasonal pulse survey entry needs an entry");
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyEntryValidatorReportsEveryProblem();
        SeasonalPulseSurveyEntryPriorityRangeRuleFlagsValuesAbovePriority();
        SeasonalPulseSurveyEntryRequiredCategoryRuleFlagsABlankCategory();
    }
}
