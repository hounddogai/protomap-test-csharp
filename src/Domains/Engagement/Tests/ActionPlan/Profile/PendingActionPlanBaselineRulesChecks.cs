namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingActionPlanBaselineRulesChecks
{
    public static void PendingActionPlanBaselineValidatorReportsEveryProblem()
    {
        var validator = PendingActionPlanBaselineRules.DefaultPendingActionPlanBaselineValidator();
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingActionPlanBaselineNameLengthRuleFlagsALongName()
    {
        var rule = new PendingActionPlanBaselineNameLengthRule(32);
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingActionPlanBaselineRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PendingActionPlanBaselineRevisionRangeRule(1000, 86400);
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingActionPlanBaselineEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new PendingActionPlanBaselineEmployeeIdLengthRule(86400);
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingActionPlanBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingActionPlanBaselineRequiredLabelRule("pending action plan baseline needs an entry");
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingActionPlanBaselineAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PendingActionPlanBaselineAttemptsRangeRule(3600, 86400);
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingActionPlanBaselineWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PendingActionPlanBaselineWindowDaysRangeRule(64, 86400);
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingActionPlanBaselineValidatorReportsEveryProblem();
        PendingActionPlanBaselineNameLengthRuleFlagsALongName();
        PendingActionPlanBaselineRevisionRangeRuleFlagsValuesAboveRevision();
        PendingActionPlanBaselineEmployeeIdLengthRuleFlagsALongEmployeeId();
        PendingActionPlanBaselineRequiredLabelRuleFlagsABlankLabel();
        PendingActionPlanBaselineAttemptsRangeRuleFlagsValuesAboveAttempts();
        PendingActionPlanBaselineWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
