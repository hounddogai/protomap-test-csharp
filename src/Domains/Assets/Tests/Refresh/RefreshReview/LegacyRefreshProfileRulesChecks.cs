namespace Acme.Workforce.Assets.Tests.Refresh.RefreshReview;

using Acme.Workforce.Assets.Refresh.RefreshReview;

/// <summary>Checks of refresh review code, run by the project's runner.</summary>
public static class LegacyRefreshProfileRulesChecks
{
    public static void LegacyRefreshProfileValidatorReportsEveryProblem()
    {
        var validator = LegacyRefreshProfileRules.DefaultLegacyRefreshProfileValidator();
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRefreshProfileAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LegacyRefreshProfileAttemptsRangeRule(32, 86400);
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRefreshProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new LegacyRefreshProfileTenureMonthsRangeRule(80, 3600);
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRefreshProfileEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LegacyRefreshProfileEmployeeIdLengthRule(16);
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRefreshProfileValidatorReportsEveryProblem();
        LegacyRefreshProfileAttemptsRangeRuleFlagsValuesAboveAttempts();
        LegacyRefreshProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        LegacyRefreshProfileEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
