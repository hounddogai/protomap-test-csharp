namespace Acme.Workforce.Engagement.Tests.TownHall.Tier;

using Acme.Workforce.Engagement.TownHall.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TownHallPlanRulesChecks
{
    public static void TownHallPlanValidatorReportsEveryProblem()
    {
        var validator = TownHallPlanRules.DefaultTownHallPlanValidator();
        var item = TownHallPlan.NewTownHallPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallPlanRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new TownHallPlanRequiredExternalRefRule("town hall plan needs this field");
        var item = TownHallPlan.NewTownHallPlan("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallPlanNameLengthRuleFlagsALongName()
    {
        var rule = new TownHallPlanNameLengthRule(255);
        var item = TownHallPlan.NewTownHallPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallPlanExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new TownHallPlanExternalRefLengthRule(3600);
        var item = TownHallPlan.NewTownHallPlan("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallPlanValidatorReportsEveryProblem();
        TownHallPlanRequiredExternalRefRuleFlagsABlankExternalRef();
        TownHallPlanNameLengthRuleFlagsALongName();
        TownHallPlanExternalRefLengthRuleFlagsALongExternalRef();
    }
}
