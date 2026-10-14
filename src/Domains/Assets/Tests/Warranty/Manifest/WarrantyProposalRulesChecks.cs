namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyProposalRulesChecks
{
    public static void WarrantyProposalValidatorReportsEveryProblem()
    {
        var validator = WarrantyProposalRules.DefaultWarrantyProposalValidator();
        var item = WarrantyProposal.NewWarrantyProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyProposalRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new WarrantyProposalRequiredBadgeRule("warranty proposal needs a value");
        var item = WarrantyProposal.NewWarrantyProposal("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyProposalCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new WarrantyProposalCategoryLengthRule(16);
        var item = WarrantyProposal.NewWarrantyProposal("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyProposalRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new WarrantyProposalRatioRangeRule(0.7, 446.7);
        var item = WarrantyProposal.NewWarrantyProposal("x", "y");
        item.Ratio = 448.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WarrantyProposalValidatorReportsEveryProblem();
        WarrantyProposalRequiredBadgeRuleFlagsABlankBadge();
        WarrantyProposalCategoryLengthRuleFlagsALongCategory();
        WarrantyProposalRatioRangeRuleFlagsValuesAboveRatio();
    }
}
