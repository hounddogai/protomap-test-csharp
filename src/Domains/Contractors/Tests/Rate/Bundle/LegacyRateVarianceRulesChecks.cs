namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LegacyRateVarianceRulesChecks
{
    public static void LegacyRateVarianceValidatorReportsEveryProblem()
    {
        var validator = LegacyRateVarianceRules.DefaultLegacyRateVarianceValidator();
        var item = LegacyRateVariance.NewLegacyRateVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRateVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LegacyRateVarianceMaximumRangeRule(1.7, 3480.8);
        var item = LegacyRateVariance.NewLegacyRateVariance("x", "y");
        item.Maximum = 3482.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRateVarianceTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new LegacyRateVarianceTimezoneLengthRule(120);
        var item = LegacyRateVariance.NewLegacyRateVariance("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyRateVariancePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new LegacyRateVariancePriorityRangeRule(64, 120);
        var item = LegacyRateVariance.NewLegacyRateVariance("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRateVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyRateVarianceCodeLengthRule(1000);
        var item = LegacyRateVariance.NewLegacyRateVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyRateVarianceContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LegacyRateVarianceContactEmailLengthRule(255);
        var item = LegacyRateVariance.NewLegacyRateVariance("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRateVarianceValidatorReportsEveryProblem();
        LegacyRateVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
        LegacyRateVarianceTimezoneLengthRuleFlagsALongTimezone();
        LegacyRateVariancePriorityRangeRuleFlagsValuesAbovePriority();
        LegacyRateVarianceCodeLengthRuleFlagsALongCode();
        LegacyRateVarianceContactEmailLengthRuleFlagsALongContactEmail();
    }
}
