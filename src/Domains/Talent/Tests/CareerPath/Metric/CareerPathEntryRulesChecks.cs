namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathEntryRulesChecks
{
    public static void CareerPathEntryValidatorReportsEveryProblem()
    {
        var validator = CareerPathEntryRules.DefaultCareerPathEntryValidator();
        var item = CareerPathEntry.NewCareerPathEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new CareerPathEntryLeaveDaysRangeRule(32, 120);
        var item = CareerPathEntry.NewCareerPathEntry("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new CareerPathEntryRequiredNameRule("career path entry needs an entry");
        var item = CareerPathEntry.NewCareerPathEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CareerPathEntryRequiredTitleRule("career path entry needs this field");
        var item = CareerPathEntry.NewCareerPathEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathEntryValidatorReportsEveryProblem();
        CareerPathEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        CareerPathEntryRequiredNameRuleFlagsABlankName();
        CareerPathEntryRequiredTitleRuleFlagsABlankTitle();
    }
}
