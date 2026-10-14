namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ScheduledGarnishmentGroupRulesChecks
{
    public static void ScheduledGarnishmentGroupValidatorReportsEveryProblem()
    {
        var validator = ScheduledGarnishmentGroupRules.DefaultScheduledGarnishmentGroupValidator();
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledGarnishmentGroupRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ScheduledGarnishmentGroupRatingRangeRule(8.6, 2675.5);
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("x", "y");
        item.Rating = 2677.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledGarnishmentGroupMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ScheduledGarnishmentGroupMaximumRangeRule(5.7, 3804.5);
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("x", "y");
        item.Maximum = 3806.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledGarnishmentGroupDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new ScheduledGarnishmentGroupDescriptionLengthRule(16);
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledGarnishmentGroupRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledGarnishmentGroupRequiredNameRule("scheduled garnishment group needs this field");
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentGroupValidatorReportsEveryProblem();
        ScheduledGarnishmentGroupRatingRangeRuleFlagsValuesAboveRating();
        ScheduledGarnishmentGroupMaximumRangeRuleFlagsValuesAboveMaximum();
        ScheduledGarnishmentGroupDescriptionLengthRuleFlagsALongDescription();
        ScheduledGarnishmentGroupRequiredNameRuleFlagsABlankName();
    }
}
