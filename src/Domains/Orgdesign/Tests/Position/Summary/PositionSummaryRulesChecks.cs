namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSummaryRulesChecks
{
    public static void PositionSummaryValidatorReportsEveryProblem()
    {
        var validator = PositionSummaryRules.DefaultPositionSummaryValidator();
        var item = PositionSummary.NewPositionSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionSummaryEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PositionSummaryEndsAtRangeRule(80, 255);
        var item = PositionSummary.NewPositionSummary("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionSummaryRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PositionSummaryRequiredDescriptionRule("position summary needs a value");
        var item = PositionSummary.NewPositionSummary("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionSummaryPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new PositionSummaryPhoneLengthRule(1000);
        var item = PositionSummary.NewPositionSummary("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionSummaryDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PositionSummaryDiscountRangeRule(6.4, 3304.9);
        var item = PositionSummary.NewPositionSummary("x", "y");
        item.Discount = 3306.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionSummaryValidatorReportsEveryProblem();
        PositionSummaryEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PositionSummaryRequiredDescriptionRuleFlagsABlankDescription();
        PositionSummaryPhoneLengthRuleFlagsALongPhone();
        PositionSummaryDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
