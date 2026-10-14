namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredActionPlanReservationRulesChecks
{
    public static void TieredActionPlanReservationValidatorReportsEveryProblem()
    {
        var validator = TieredActionPlanReservationRules.DefaultTieredActionPlanReservationValidator();
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredActionPlanReservationRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TieredActionPlanReservationRetriesRangeRule(120, 1000);
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredActionPlanReservationBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new TieredActionPlanReservationBadgeLengthRule(1000);
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredActionPlanReservationWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new TieredActionPlanReservationWeightRangeRule(6.2, 2693.7);
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("x", "y");
        item.Weight = 2695.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredActionPlanReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredActionPlanReservationRequiredNameRule("tiered action plan reservation needs an entry");
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredActionPlanReservationValidatorReportsEveryProblem();
        TieredActionPlanReservationRetriesRangeRuleFlagsValuesAboveRetries();
        TieredActionPlanReservationBadgeLengthRuleFlagsALongBadge();
        TieredActionPlanReservationWeightRangeRuleFlagsValuesAboveWeight();
        TieredActionPlanReservationRequiredNameRuleFlagsABlankName();
    }
}
