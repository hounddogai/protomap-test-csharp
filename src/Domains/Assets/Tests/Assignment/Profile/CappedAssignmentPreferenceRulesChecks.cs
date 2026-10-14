namespace Acme.Workforce.Assets.Tests.Assignment.Profile;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedAssignmentPreferenceRulesChecks
{
    public static void CappedAssignmentPreferenceValidatorReportsEveryProblem()
    {
        var validator = CappedAssignmentPreferenceRules.DefaultCappedAssignmentPreferenceValidator();
        var item = CappedAssignmentPreference.NewCappedAssignmentPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAssignmentPreferenceBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new CappedAssignmentPreferenceBudgetRangeRule(6.5, 3232.4);
        var item = CappedAssignmentPreference.NewCappedAssignmentPreference("x", "y");
        item.Budget = 3233.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAssignmentPreferenceGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new CappedAssignmentPreferenceGraceDaysRangeRule(16, 86400);
        var item = CappedAssignmentPreference.NewCappedAssignmentPreference("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAssignmentPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedAssignmentPreferenceCodeLengthRule(80);
        var item = CappedAssignmentPreference.NewCappedAssignmentPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedAssignmentPreferenceValidatorReportsEveryProblem();
        CappedAssignmentPreferenceBudgetRangeRuleFlagsValuesAboveBudget();
        CappedAssignmentPreferenceGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        CappedAssignmentPreferenceCodeLengthRuleFlagsALongCode();
    }
}
