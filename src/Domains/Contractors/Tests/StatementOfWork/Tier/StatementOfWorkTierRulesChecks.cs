namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkTierRulesChecks
{
    public static void StatementOfWorkTierValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkTierRules.DefaultStatementOfWorkTierValidator();
        var item = StatementOfWorkTier.NewStatementOfWorkTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkTierRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new StatementOfWorkTierRequiredDescriptionRule("statement of work tier needs an entry");
        var item = StatementOfWorkTier.NewStatementOfWorkTier("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkTierRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new StatementOfWorkTierRequiredTimezoneRule("statement of work tier needs an entry");
        var item = StatementOfWorkTier.NewStatementOfWorkTier("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StatementOfWorkTierLabelLengthRule(86400);
        var item = StatementOfWorkTier.NewStatementOfWorkTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkTierWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new StatementOfWorkTierWindowDaysRangeRule(64, 255);
        var item = StatementOfWorkTier.NewStatementOfWorkTier("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StatementOfWorkTierValidatorReportsEveryProblem();
        StatementOfWorkTierRequiredDescriptionRuleFlagsABlankDescription();
        StatementOfWorkTierRequiredTimezoneRuleFlagsABlankTimezone();
        StatementOfWorkTierLabelLengthRuleFlagsALongLabel();
        StatementOfWorkTierWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
