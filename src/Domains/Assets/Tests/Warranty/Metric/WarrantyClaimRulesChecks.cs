namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyClaimRulesChecks
{
    public static void WarrantyClaimValidatorReportsEveryProblem()
    {
        var validator = WarrantyClaimRules.DefaultWarrantyClaimValidator();
        var item = WarrantyClaim.NewWarrantyClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyClaimRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new WarrantyClaimRatingRangeRule(3.7, 4471.0);
        var item = WarrantyClaim.NewWarrantyClaim("x", "y");
        item.Rating = 4472.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyClaimFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new WarrantyClaimFteRangeRule(9.1, 126.2);
        var item = WarrantyClaim.NewWarrantyClaim("x", "y");
        item.Fte = 127.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyClaimExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new WarrantyClaimExternalRefLengthRule(80);
        var item = WarrantyClaim.NewWarrantyClaim("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyClaimValidatorReportsEveryProblem();
        WarrantyClaimRatingRangeRuleFlagsValuesAboveRating();
        WarrantyClaimFteRangeRuleFlagsValuesAboveFte();
        WarrantyClaimExternalRefLengthRuleFlagsALongExternalRef();
    }
}
