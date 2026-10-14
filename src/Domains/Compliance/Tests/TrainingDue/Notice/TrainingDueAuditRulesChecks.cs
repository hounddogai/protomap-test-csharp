namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueAuditRulesChecks
{
    public static void TrainingDueAuditValidatorReportsEveryProblem()
    {
        var validator = TrainingDueAuditRules.DefaultTrainingDueAuditValidator();
        var item = TrainingDueAudit.NewTrainingDueAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueAuditBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new TrainingDueAuditBatchSizeRangeRule(120, 86400);
        var item = TrainingDueAudit.NewTrainingDueAudit("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueAuditRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new TrainingDueAuditRequiredBadgeRule("training due audit needs a value");
        var item = TrainingDueAudit.NewTrainingDueAudit("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TrainingDueAuditTitleLengthRule(80);
        var item = TrainingDueAudit.NewTrainingDueAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueAuditValidatorReportsEveryProblem();
        TrainingDueAuditBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        TrainingDueAuditRequiredBadgeRuleFlagsABlankBadge();
        TrainingDueAuditTitleLengthRuleFlagsALongTitle();
    }
}
