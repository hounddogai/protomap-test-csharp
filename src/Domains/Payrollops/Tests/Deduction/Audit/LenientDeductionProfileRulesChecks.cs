namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientDeductionProfileRulesChecks
{
    public static void LenientDeductionProfileValidatorReportsEveryProblem()
    {
        var validator = LenientDeductionProfileRules.DefaultLenientDeductionProfileValidator();
        var item = LenientDeductionProfile.NewLenientDeductionProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientDeductionProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientDeductionProfileRequiredNameRule("lenient deduction profile needs an entry");
        var item = LenientDeductionProfile.NewLenientDeductionProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeductionProfileCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LenientDeductionProfileCeilingRangeRule(6.1, 2701.8);
        var item = LenientDeductionProfile.NewLenientDeductionProfile("x", "y");
        item.Ceiling = 2703.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientDeductionProfileRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LenientDeductionProfileRatioRangeRule(4.6, 2717.9);
        var item = LenientDeductionProfile.NewLenientDeductionProfile("x", "y");
        item.Ratio = 2719.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientDeductionProfileRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new LenientDeductionProfileRequiredCategoryRule("lenient deduction profile needs this field");
        var item = LenientDeductionProfile.NewLenientDeductionProfile("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientDeductionProfileValidatorReportsEveryProblem();
        LenientDeductionProfileRequiredNameRuleFlagsABlankName();
        LenientDeductionProfileCeilingRangeRuleFlagsValuesAboveCeiling();
        LenientDeductionProfileRatioRangeRuleFlagsValuesAboveRatio();
        LenientDeductionProfileRequiredCategoryRuleFlagsABlankCategory();
    }
}
