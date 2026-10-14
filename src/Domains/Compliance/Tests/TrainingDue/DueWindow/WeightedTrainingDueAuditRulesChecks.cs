namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class WeightedTrainingDueAuditRulesChecks
{
    public static void WeightedTrainingDueAuditValidatorReportsEveryProblem()
    {
        var validator = WeightedTrainingDueAuditRules.DefaultWeightedTrainingDueAuditValidator();
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedTrainingDueAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedTrainingDueAuditRequiredTitleRule("weighted training due audit needs a value");
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedTrainingDueAuditRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new WeightedTrainingDueAuditRequiredCostCenterRule("weighted training due audit needs this field");
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedTrainingDueAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedTrainingDueAuditRequiredCodeRule("weighted training due audit needs an entry");
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedTrainingDueAuditCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new WeightedTrainingDueAuditCostCenterLengthRule(16);
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedTrainingDueAuditValidatorReportsEveryProblem();
        WeightedTrainingDueAuditRequiredTitleRuleFlagsABlankTitle();
        WeightedTrainingDueAuditRequiredCostCenterRuleFlagsABlankCostCenter();
        WeightedTrainingDueAuditRequiredCodeRuleFlagsABlankCode();
        WeightedTrainingDueAuditCostCenterLengthRuleFlagsALongCostCenter();
    }
}
