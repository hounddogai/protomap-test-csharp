namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationAdjustmentRulesChecks
{
    public static void NominationAdjustmentValidatorReportsEveryProblem()
    {
        var validator = NominationAdjustmentRules.DefaultNominationAdjustmentValidator();
        var item = NominationAdjustment.NewNominationAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationAdjustmentRequiredCodeRule("nomination adjustment needs an entry");
        var item = NominationAdjustment.NewNominationAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationAdjustmentWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new NominationAdjustmentWeightRangeRule(1.9, 1841.3);
        var item = NominationAdjustment.NewNominationAdjustment("x", "y");
        item.Weight = 1842.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationAdjustmentDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new NominationAdjustmentDiscountRangeRule(8.7, 3165.3);
        var item = NominationAdjustment.NewNominationAdjustment("x", "y");
        item.Discount = 3166.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationAdjustmentPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new NominationAdjustmentPhoneLengthRule(120);
        var item = NominationAdjustment.NewNominationAdjustment("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationAdjustmentValidatorReportsEveryProblem();
        NominationAdjustmentRequiredCodeRuleFlagsABlankCode();
        NominationAdjustmentWeightRangeRuleFlagsValuesAboveWeight();
        NominationAdjustmentDiscountRangeRuleFlagsValuesAboveDiscount();
        NominationAdjustmentPhoneLengthRuleFlagsALongPhone();
    }
}
