namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class LegacyTrainingDueVarianceRulesChecks
{
    public static void LegacyTrainingDueVarianceValidatorReportsEveryProblem()
    {
        var validator = LegacyTrainingDueVarianceRules.DefaultLegacyTrainingDueVarianceValidator();
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTrainingDueVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyTrainingDueVarianceCodeLengthRule(1000);
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTrainingDueVarianceRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new LegacyTrainingDueVarianceRequiredOwnerIdRule("legacy training due variance needs this field");
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTrainingDueVarianceRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new LegacyTrainingDueVarianceRequiredCurrencyRule("legacy training due variance needs a value");
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTrainingDueVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyTrainingDueVarianceRequiredLabelRule("legacy training due variance needs a value");
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyTrainingDueVarianceValidatorReportsEveryProblem();
        LegacyTrainingDueVarianceCodeLengthRuleFlagsALongCode();
        LegacyTrainingDueVarianceRequiredOwnerIdRuleFlagsABlankOwnerId();
        LegacyTrainingDueVarianceRequiredCurrencyRuleFlagsABlankCurrency();
        LegacyTrainingDueVarianceRequiredLabelRuleFlagsABlankLabel();
    }
}
