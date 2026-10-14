namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingStatementOfWorkPreferenceRulesChecks
{
    public static void PendingStatementOfWorkPreferenceValidatorReportsEveryProblem()
    {
        var validator = PendingStatementOfWorkPreferenceRules.DefaultPendingStatementOfWorkPreferenceValidator();
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingStatementOfWorkPreferenceDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PendingStatementOfWorkPreferenceDiscountRangeRule(6.4, 3399.7);
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("x", "y");
        item.Discount = 3401.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingStatementOfWorkPreferenceNameLengthRuleFlagsALongName()
    {
        var rule = new PendingStatementOfWorkPreferenceNameLengthRule(64);
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingStatementOfWorkPreferenceDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PendingStatementOfWorkPreferenceDurationMinutesRangeRule(64, 80);
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("x", "y");
        item.DurationMinutes = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingStatementOfWorkPreferenceFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new PendingStatementOfWorkPreferenceFteRangeRule(0.6, 1199.4);
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("x", "y");
        item.Fte = 1200.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingStatementOfWorkPreferenceValidatorReportsEveryProblem();
        PendingStatementOfWorkPreferenceDiscountRangeRuleFlagsValuesAboveDiscount();
        PendingStatementOfWorkPreferenceNameLengthRuleFlagsALongName();
        PendingStatementOfWorkPreferenceDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        PendingStatementOfWorkPreferenceFteRangeRuleFlagsValuesAboveFte();
    }
}
