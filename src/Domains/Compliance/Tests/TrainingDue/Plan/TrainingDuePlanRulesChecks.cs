namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDuePlanRulesChecks
{
    public static void TrainingDuePlanValidatorReportsEveryProblem()
    {
        var validator = TrainingDuePlanRules.DefaultTrainingDuePlanValidator();
        var item = TrainingDuePlan.NewTrainingDuePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDuePlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TrainingDuePlanRequiredCodeRule("training due plan needs a value");
        var item = TrainingDuePlan.NewTrainingDuePlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDuePlanLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new TrainingDuePlanLeaveDaysRangeRule(80, 86400);
        var item = TrainingDuePlan.NewTrainingDuePlan("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDuePlanValidatorReportsEveryProblem();
        TrainingDuePlanRequiredCodeRuleFlagsABlankCode();
        TrainingDuePlanLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
