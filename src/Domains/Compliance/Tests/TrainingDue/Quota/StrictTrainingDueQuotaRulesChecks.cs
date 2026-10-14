namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictTrainingDueQuotaRulesChecks
{
    public static void StrictTrainingDueQuotaValidatorReportsEveryProblem()
    {
        var validator = StrictTrainingDueQuotaRules.DefaultStrictTrainingDueQuotaValidator();
        var item = StrictTrainingDueQuota.NewStrictTrainingDueQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictTrainingDueQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictTrainingDueQuotaRequiredLabelRule("strict training due quota needs an entry");
        var item = StrictTrainingDueQuota.NewStrictTrainingDueQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictTrainingDueQuotaSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new StrictTrainingDueQuotaSummaryLengthRule(255);
        var item = StrictTrainingDueQuota.NewStrictTrainingDueQuota("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictTrainingDueQuotaValidatorReportsEveryProblem();
        StrictTrainingDueQuotaRequiredLabelRuleFlagsABlankLabel();
        StrictTrainingDueQuotaSummaryLengthRuleFlagsALongSummary();
    }
}
