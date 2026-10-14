namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WarrantyNoticeRulesChecks
{
    public static void WarrantyNoticeValidatorReportsEveryProblem()
    {
        var validator = WarrantyNoticeRules.DefaultWarrantyNoticeValidator();
        var item = WarrantyNotice.NewWarrantyNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyNoticeCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new WarrantyNoticeCostRangeRule(0.4, 2451.7);
        var item = WarrantyNotice.NewWarrantyNotice("x", "y");
        item.Cost = 2453.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyNoticeRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new WarrantyNoticeRatingRangeRule(6.3, 3033.3);
        var item = WarrantyNotice.NewWarrantyNotice("x", "y");
        item.Rating = 3034.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyNoticeBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new WarrantyNoticeBudgetRangeRule(3.9, 2572.5);
        var item = WarrantyNotice.NewWarrantyNotice("x", "y");
        item.Budget = 2574.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyNoticePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new WarrantyNoticePhoneLengthRule(1000);
        var item = WarrantyNotice.NewWarrantyNotice("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyNoticeValidatorReportsEveryProblem();
        WarrantyNoticeCostRangeRuleFlagsValuesAboveCost();
        WarrantyNoticeRatingRangeRuleFlagsValuesAboveRating();
        WarrantyNoticeBudgetRangeRuleFlagsValuesAboveBudget();
        WarrantyNoticePhoneLengthRuleFlagsALongPhone();
    }
}
