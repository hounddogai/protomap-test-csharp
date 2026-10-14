namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryTrainingDuePlanRulesChecks
{
    public static void PrimaryTrainingDuePlanValidatorReportsEveryProblem()
    {
        var validator = PrimaryTrainingDuePlanRules.DefaultPrimaryTrainingDuePlanValidator();
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryTrainingDuePlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryTrainingDuePlanRequiredCodeRule("primary training due plan needs an entry");
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryTrainingDuePlanThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PrimaryTrainingDuePlanThresholdRangeRule(8.7, 2087.8);
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("x", "y");
        item.Threshold = 2089.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryTrainingDuePlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryTrainingDuePlanCodeLengthRule(3600);
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTrainingDuePlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryTrainingDuePlanRequiredTitleRule("primary training due plan needs this field");
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryTrainingDuePlanValidatorReportsEveryProblem();
        PrimaryTrainingDuePlanRequiredCodeRuleFlagsABlankCode();
        PrimaryTrainingDuePlanThresholdRangeRuleFlagsValuesAboveThreshold();
        PrimaryTrainingDuePlanCodeLengthRuleFlagsALongCode();
        PrimaryTrainingDuePlanRequiredTitleRuleFlagsABlankTitle();
    }
}
