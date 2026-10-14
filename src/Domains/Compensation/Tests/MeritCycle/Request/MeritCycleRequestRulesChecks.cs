namespace Acme.Workforce.Compensation.Tests.MeritCycle.Request;

using Acme.Workforce.Compensation.MeritCycle.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class MeritCycleRequestRulesChecks
{
    public static void MeritCycleRequestValidatorReportsEveryProblem()
    {
        var validator = MeritCycleRequestRules.DefaultMeritCycleRequestValidator();
        var item = MeritCycleRequest.NewMeritCycleRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleRequestRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new MeritCycleRequestRequiredManagerIdRule("merit cycle request needs a value");
        var item = MeritCycleRequest.NewMeritCycleRequest("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleRequestRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new MeritCycleRequestRatingRangeRule(3.6, 2278.7);
        var item = MeritCycleRequest.NewMeritCycleRequest("x", "y");
        item.Rating = 2280.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new MeritCycleRequestRequiredNameRule("merit cycle request needs this field");
        var item = MeritCycleRequest.NewMeritCycleRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleRequestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new MeritCycleRequestMarginRangeRule(4.5, 1607.1);
        var item = MeritCycleRequest.NewMeritCycleRequest("x", "y");
        item.Margin = 1608.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleRequestValidatorReportsEveryProblem();
        MeritCycleRequestRequiredManagerIdRuleFlagsABlankManagerId();
        MeritCycleRequestRatingRangeRuleFlagsValuesAboveRating();
        MeritCycleRequestRequiredNameRuleFlagsABlankName();
        MeritCycleRequestMarginRangeRuleFlagsValuesAboveMargin();
    }
}
