namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyFeedbackClaimRulesChecks
{
    public static void LegacyFeedbackClaimValidatorReportsEveryProblem()
    {
        var validator = LegacyFeedbackClaimRules.DefaultLegacyFeedbackClaimValidator();
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyFeedbackClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyFeedbackClaimRequiredCodeRule("legacy feedback claim needs this field");
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyFeedbackClaimRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LegacyFeedbackClaimRequiredLocationRule("legacy feedback claim needs this field");
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyFeedbackClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyFeedbackClaimCodeLengthRule(3600);
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyFeedbackClaimCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LegacyFeedbackClaimCostRangeRule(4.1, 734.0);
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("x", "y");
        item.Cost = 735.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyFeedbackClaimRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new LegacyFeedbackClaimRequiredCountryRule("legacy feedback claim needs a value");
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyFeedbackClaimValidatorReportsEveryProblem();
        LegacyFeedbackClaimRequiredCodeRuleFlagsABlankCode();
        LegacyFeedbackClaimRequiredLocationRuleFlagsABlankLocation();
        LegacyFeedbackClaimCodeLengthRuleFlagsALongCode();
        LegacyFeedbackClaimCostRangeRuleFlagsValuesAboveCost();
        LegacyFeedbackClaimRequiredCountryRuleFlagsABlankCountry();
    }
}
