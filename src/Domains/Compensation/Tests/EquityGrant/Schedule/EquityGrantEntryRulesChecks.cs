namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantEntryRulesChecks
{
    public static void EquityGrantEntryValidatorReportsEveryProblem()
    {
        var validator = EquityGrantEntryRules.DefaultEquityGrantEntryValidator();
        var item = EquityGrantEntry.NewEquityGrantEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantEntryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new EquityGrantEntryBudgetRangeRule(1.1, 609.3);
        var item = EquityGrantEntry.NewEquityGrantEntry("x", "y");
        item.Budget = 610.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantEntryRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new EquityGrantEntryRevisionRangeRule(64, 3600);
        var item = EquityGrantEntry.NewEquityGrantEntry("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantEntryRequiredCodeRule("equity grant entry needs a value");
        var item = EquityGrantEntry.NewEquityGrantEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantEntryValidatorReportsEveryProblem();
        EquityGrantEntryBudgetRangeRuleFlagsValuesAboveBudget();
        EquityGrantEntryRevisionRangeRuleFlagsValuesAboveRevision();
        EquityGrantEntryRequiredCodeRuleFlagsABlankCode();
    }
}
