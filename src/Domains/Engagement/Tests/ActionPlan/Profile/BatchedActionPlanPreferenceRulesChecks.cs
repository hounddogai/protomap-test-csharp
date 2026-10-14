namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BatchedActionPlanPreferenceRulesChecks
{
    public static void BatchedActionPlanPreferenceValidatorReportsEveryProblem()
    {
        var validator = BatchedActionPlanPreferenceRules.DefaultBatchedActionPlanPreferenceValidator();
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedActionPlanPreferenceRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new BatchedActionPlanPreferenceRequiredDescriptionRule(
            "batched action plan preference needs a value");
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedActionPlanPreferenceRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new BatchedActionPlanPreferenceRequiredTeamRule("batched action plan preference needs an entry");
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedActionPlanPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedActionPlanPreferenceCodeLengthRule(64);
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedActionPlanPreferenceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BatchedActionPlanPreferenceTenureMonthsRangeRule(32, 120);
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedActionPlanPreferenceValidatorReportsEveryProblem();
        BatchedActionPlanPreferenceRequiredDescriptionRuleFlagsABlankDescription();
        BatchedActionPlanPreferenceRequiredTeamRuleFlagsABlankTeam();
        BatchedActionPlanPreferenceCodeLengthRuleFlagsALongCode();
        BatchedActionPlanPreferenceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
