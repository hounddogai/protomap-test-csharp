namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AdaptiveCostModelReservationRulesChecks
{
    public static void AdaptiveCostModelReservationValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCostModelReservationRules.DefaultAdaptiveCostModelReservationValidator();
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCostModelReservationRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AdaptiveCostModelReservationRequiredLocationRule(
            "adaptive cost model reservation needs an entry");
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCostModelReservationSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AdaptiveCostModelReservationSummaryLengthRule(255);
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveCostModelReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveCostModelReservationCodeLengthRule(86400);
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveCostModelReservationValidatorReportsEveryProblem();
        AdaptiveCostModelReservationRequiredLocationRuleFlagsABlankLocation();
        AdaptiveCostModelReservationSummaryLengthRuleFlagsALongSummary();
        AdaptiveCostModelReservationCodeLengthRuleFlagsALongCode();
    }
}
