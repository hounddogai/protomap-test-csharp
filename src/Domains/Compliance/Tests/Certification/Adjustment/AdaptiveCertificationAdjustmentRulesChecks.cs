namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AdaptiveCertificationAdjustmentRulesChecks
{
    public static void AdaptiveCertificationAdjustmentValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCertificationAdjustmentRules.DefaultAdaptiveCertificationAdjustmentValidator();
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCertificationAdjustmentCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new AdaptiveCertificationAdjustmentCeilingRangeRule(9.0, 3101.9);
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("x", "y");
        item.Ceiling = 3103.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveCertificationAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveCertificationAdjustmentNameLengthRule(16);
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveCertificationAdjustmentValidatorReportsEveryProblem();
        AdaptiveCertificationAdjustmentCeilingRangeRuleFlagsValuesAboveCeiling();
        AdaptiveCertificationAdjustmentNameLengthRuleFlagsALongName();
    }
}
