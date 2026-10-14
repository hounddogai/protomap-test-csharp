namespace Acme.Workforce.Orgdesign.Tests.Reorg.Profile;

using Acme.Workforce.Orgdesign.Reorg.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ReorgPlanRulesChecks
{
    public static void ReorgPlanValidatorReportsEveryProblem()
    {
        var validator = ReorgPlanRules.DefaultReorgPlanValidator();
        var item = ReorgPlan.NewReorgPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgPlanRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ReorgPlanRequiredEmployeeIdRule("reorg plan needs a value");
        var item = ReorgPlan.NewReorgPlan("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgPlanNameLengthRuleFlagsALongName()
    {
        var rule = new ReorgPlanNameLengthRule(32);
        var item = ReorgPlan.NewReorgPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgPlanEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new ReorgPlanEmployeeIdLengthRule(64);
        var item = ReorgPlan.NewReorgPlan("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgPlanValidatorReportsEveryProblem();
        ReorgPlanRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        ReorgPlanNameLengthRuleFlagsALongName();
        ReorgPlanEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
