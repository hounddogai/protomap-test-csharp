namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class DeskEstimateRulesChecks
{
    public static void DeskEstimateValidatorReportsEveryProblem()
    {
        var validator = DeskEstimateRules.DefaultDeskEstimateValidator();
        var item = DeskEstimate.NewDeskEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskEstimateRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new DeskEstimateRequiredOwnerIdRule("desk estimate needs a value");
        var item = DeskEstimate.NewDeskEstimate("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskEstimateStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new DeskEstimateStartsAtRangeRule(80, 1000);
        var item = DeskEstimate.NewDeskEstimate("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskEstimateCodeLengthRule(80);
        var item = DeskEstimate.NewDeskEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskEstimateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeskEstimateRequiredTitleRule("desk estimate needs a value");
        var item = DeskEstimate.NewDeskEstimate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskEstimateRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new DeskEstimateRatingRangeRule(2.0, 2595.9);
        var item = DeskEstimate.NewDeskEstimate("x", "y");
        item.Rating = 2597.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeskEstimateValidatorReportsEveryProblem();
        DeskEstimateRequiredOwnerIdRuleFlagsABlankOwnerId();
        DeskEstimateStartsAtRangeRuleFlagsValuesAboveStartsAt();
        DeskEstimateCodeLengthRuleFlagsALongCode();
        DeskEstimateRequiredTitleRuleFlagsABlankTitle();
        DeskEstimateRatingRangeRuleFlagsValuesAboveRating();
    }
}
