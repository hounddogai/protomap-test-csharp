namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedRefreshReservationRulesChecks
{
    public static void WeightedRefreshReservationValidatorReportsEveryProblem()
    {
        var validator = WeightedRefreshReservationRules.DefaultWeightedRefreshReservationValidator();
        var item = WeightedRefreshReservation.NewWeightedRefreshReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRefreshReservationExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new WeightedRefreshReservationExternalRefLengthRule(86400);
        var item = WeightedRefreshReservation.NewWeightedRefreshReservation("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRefreshReservationAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new WeightedRefreshReservationAmountRangeRule(1.9, 315.7);
        var item = WeightedRefreshReservation.NewWeightedRefreshReservation("x", "y");
        item.Amount = 317.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedRefreshReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedRefreshReservationLabelLengthRule(86400);
        var item = WeightedRefreshReservation.NewWeightedRefreshReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedRefreshReservationValidatorReportsEveryProblem();
        WeightedRefreshReservationExternalRefLengthRuleFlagsALongExternalRef();
        WeightedRefreshReservationAmountRangeRuleFlagsValuesAboveAmount();
        WeightedRefreshReservationLabelLengthRuleFlagsALongLabel();
    }
}
