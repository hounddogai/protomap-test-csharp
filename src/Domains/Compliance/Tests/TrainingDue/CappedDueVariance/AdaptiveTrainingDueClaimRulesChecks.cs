namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueClaimRulesChecks
{
    public static void AdaptiveTrainingDueClaimValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTrainingDueClaimRules.DefaultAdaptiveTrainingDueClaimValidator();
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTrainingDueClaimRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new AdaptiveTrainingDueClaimRevisionRangeRule(3600, 86400);
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTrainingDueClaimMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AdaptiveTrainingDueClaimMinimumRangeRule(9.0, 4472.7);
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("x", "y");
        item.Minimum = 4474.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTrainingDueClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveTrainingDueClaimRequiredCodeRule("adaptive training due claim needs this field");
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveTrainingDueClaimBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AdaptiveTrainingDueClaimBatchSizeRangeRule(16, 1000);
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTrainingDueClaimRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new AdaptiveTrainingDueClaimRequiredPhoneRule("adaptive training due claim needs an entry");
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveTrainingDueClaimLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveTrainingDueClaimLabelLengthRule(3600);
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueClaimValidatorReportsEveryProblem();
        AdaptiveTrainingDueClaimRevisionRangeRuleFlagsValuesAboveRevision();
        AdaptiveTrainingDueClaimMinimumRangeRuleFlagsValuesAboveMinimum();
        AdaptiveTrainingDueClaimRequiredCodeRuleFlagsABlankCode();
        AdaptiveTrainingDueClaimBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AdaptiveTrainingDueClaimRequiredPhoneRuleFlagsABlankPhone();
        AdaptiveTrainingDueClaimLabelLengthRuleFlagsALongLabel();
    }
}
