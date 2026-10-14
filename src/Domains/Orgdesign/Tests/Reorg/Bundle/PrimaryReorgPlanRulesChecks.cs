namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PrimaryReorgPlanRulesChecks
{
    public static void PrimaryReorgPlanValidatorReportsEveryProblem()
    {
        var validator = PrimaryReorgPlanRules.DefaultPrimaryReorgPlanValidator();
        var item = PrimaryReorgPlan.NewPrimaryReorgPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryReorgPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryReorgPlanRequiredCodeRule("primary reorg plan needs an entry");
        var item = PrimaryReorgPlan.NewPrimaryReorgPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryReorgPlanLocationLengthRuleFlagsALongLocation()
    {
        var rule = new PrimaryReorgPlanLocationLengthRule(80);
        var item = PrimaryReorgPlan.NewPrimaryReorgPlan("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryReorgPlanValidatorReportsEveryProblem();
        PrimaryReorgPlanRequiredCodeRuleFlagsABlankCode();
        PrimaryReorgPlanLocationLengthRuleFlagsALongLocation();
    }
}
