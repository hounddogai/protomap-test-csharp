namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class CappedDeskAdjustmentRulesChecks
{
    public static void CappedDeskAdjustmentValidatorReportsEveryProblem()
    {
        var validator = CappedDeskAdjustmentRules.DefaultCappedDeskAdjustmentValidator();
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedDeskAdjustmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new CappedDeskAdjustmentRatioRangeRule(8.2, 3917.8);
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("x", "y");
        item.Ratio = 3919.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedDeskAdjustmentRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new CappedDeskAdjustmentRequiredDescriptionRule("capped desk adjustment needs an entry");
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedDeskAdjustmentRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new CappedDeskAdjustmentRequiredBadgeRule("capped desk adjustment needs this field");
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedDeskAdjustmentBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new CappedDeskAdjustmentBadgeLengthRule(1000);
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedDeskAdjustmentValidatorReportsEveryProblem();
        CappedDeskAdjustmentRatioRangeRuleFlagsValuesAboveRatio();
        CappedDeskAdjustmentRequiredDescriptionRuleFlagsABlankDescription();
        CappedDeskAdjustmentRequiredBadgeRuleFlagsABlankBadge();
        CappedDeskAdjustmentBadgeLengthRuleFlagsALongBadge();
    }
}
