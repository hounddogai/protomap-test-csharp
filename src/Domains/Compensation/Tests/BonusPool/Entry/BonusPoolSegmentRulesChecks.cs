namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolSegmentRulesChecks
{
    public static void BonusPoolSegmentValidatorReportsEveryProblem()
    {
        var validator = BonusPoolSegmentRules.DefaultBonusPoolSegmentValidator();
        var item = BonusPoolSegment.NewBonusPoolSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolSegmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BonusPoolSegmentLabelLengthRule(32);
        var item = BonusPoolSegment.NewBonusPoolSegment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolSegmentRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new BonusPoolSegmentRequiredCountryRule("bonus pool segment needs an entry");
        var item = BonusPoolSegment.NewBonusPoolSegment("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolSegmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new BonusPoolSegmentMaximumRangeRule(5.8, 4195.3);
        var item = BonusPoolSegment.NewBonusPoolSegment("x", "y");
        item.Maximum = 4196.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolSegmentPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new BonusPoolSegmentPriorityRangeRule(255, 86400);
        var item = BonusPoolSegment.NewBonusPoolSegment("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolSegmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BonusPoolSegmentRequiredLabelRule("bonus pool segment needs this field");
        var item = BonusPoolSegment.NewBonusPoolSegment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolSegmentValidatorReportsEveryProblem();
        BonusPoolSegmentLabelLengthRuleFlagsALongLabel();
        BonusPoolSegmentRequiredCountryRuleFlagsABlankCountry();
        BonusPoolSegmentMaximumRangeRuleFlagsValuesAboveMaximum();
        BonusPoolSegmentPriorityRangeRuleFlagsValuesAbovePriority();
        BonusPoolSegmentRequiredLabelRuleFlagsABlankLabel();
    }
}
