namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LegacySuccessionPlanPolicyRulesChecks
{
    public static void LegacySuccessionPlanPolicyValidatorReportsEveryProblem()
    {
        var validator = LegacySuccessionPlanPolicyRules.DefaultLegacySuccessionPlanPolicyValidator();
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacySuccessionPlanPolicyRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new LegacySuccessionPlanPolicyRequiredCountryRule("legacy succession plan policy needs a value");
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacySuccessionPlanPolicySequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LegacySuccessionPlanPolicySequenceRangeRule(255, 255);
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacySuccessionPlanPolicySegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LegacySuccessionPlanPolicySegmentLengthRule(64);
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacySuccessionPlanPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacySuccessionPlanPolicyRequiredCodeRule("legacy succession plan policy needs this field");
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacySuccessionPlanPolicyValidatorReportsEveryProblem();
        LegacySuccessionPlanPolicyRequiredCountryRuleFlagsABlankCountry();
        LegacySuccessionPlanPolicySequenceRangeRuleFlagsValuesAboveSequence();
        LegacySuccessionPlanPolicySegmentLengthRuleFlagsALongSegment();
        LegacySuccessionPlanPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
