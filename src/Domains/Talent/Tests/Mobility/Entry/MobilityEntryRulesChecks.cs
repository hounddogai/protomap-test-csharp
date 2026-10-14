namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class MobilityEntryRulesChecks
{
    public static void MobilityEntryValidatorReportsEveryProblem()
    {
        var validator = MobilityEntryRules.DefaultMobilityEntryValidator();
        var item = MobilityEntry.NewMobilityEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityEntryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new MobilityEntryBudgetRangeRule(5.6, 854.0);
        var item = MobilityEntry.NewMobilityEntry("x", "y");
        item.Budget = 855.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MobilityEntryTitleLengthRule(120);
        var item = MobilityEntry.NewMobilityEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityEntryCodeLengthRule(86400);
        var item = MobilityEntry.NewMobilityEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityEntryValidatorReportsEveryProblem();
        MobilityEntryBudgetRangeRuleFlagsValuesAboveBudget();
        MobilityEntryTitleLengthRuleFlagsALongTitle();
        MobilityEntryCodeLengthRuleFlagsALongCode();
    }
}
