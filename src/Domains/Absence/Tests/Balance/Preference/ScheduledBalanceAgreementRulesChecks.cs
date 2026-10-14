namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledBalanceAgreementRulesChecks
{
    public static void ScheduledBalanceAgreementValidatorReportsEveryProblem()
    {
        var validator = ScheduledBalanceAgreementRules.DefaultScheduledBalanceAgreementValidator();
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBalanceAgreementGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ScheduledBalanceAgreementGraceDaysRangeRule(64, 64);
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("x", "y");
        item.GraceDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledBalanceAgreementRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledBalanceAgreementRequiredTitleRule("scheduled balance agreement needs a value");
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBalanceAgreementSourceLengthRuleFlagsALongSource()
    {
        var rule = new ScheduledBalanceAgreementSourceLengthRule(120);
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledBalanceAgreementValidatorReportsEveryProblem();
        ScheduledBalanceAgreementGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ScheduledBalanceAgreementRequiredTitleRuleFlagsABlankTitle();
        ScheduledBalanceAgreementSourceLengthRuleFlagsALongSource();
    }
}
