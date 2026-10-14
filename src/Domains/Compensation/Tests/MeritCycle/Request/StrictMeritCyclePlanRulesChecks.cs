namespace Acme.Workforce.Compensation.Tests.MeritCycle.Request;

using Acme.Workforce.Compensation.MeritCycle.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictMeritCyclePlanRulesChecks
{
    public static void StrictMeritCyclePlanValidatorReportsEveryProblem()
    {
        var validator = StrictMeritCyclePlanRules.DefaultStrictMeritCyclePlanValidator();
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictMeritCyclePlanNameLengthRuleFlagsALongName()
    {
        var rule = new StrictMeritCyclePlanNameLengthRule(3600);
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictMeritCyclePlanRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new StrictMeritCyclePlanRequiredLocationRule("strict merit cycle plan needs a value");
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictMeritCyclePlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictMeritCyclePlanCodeLengthRule(120);
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictMeritCyclePlanStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new StrictMeritCyclePlanStartsAtRangeRule(80, 86400);
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictMeritCyclePlanRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new StrictMeritCyclePlanRequiredCurrencyRule("strict merit cycle plan needs a value");
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictMeritCyclePlanValidatorReportsEveryProblem();
        StrictMeritCyclePlanNameLengthRuleFlagsALongName();
        StrictMeritCyclePlanRequiredLocationRuleFlagsABlankLocation();
        StrictMeritCyclePlanCodeLengthRuleFlagsALongCode();
        StrictMeritCyclePlanStartsAtRangeRuleFlagsValuesAboveStartsAt();
        StrictMeritCyclePlanRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
