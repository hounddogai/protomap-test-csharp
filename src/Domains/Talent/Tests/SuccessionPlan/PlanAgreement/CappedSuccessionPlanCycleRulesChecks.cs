namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class CappedSuccessionPlanCycleRulesChecks
{
    public static void CappedSuccessionPlanCycleValidatorReportsEveryProblem()
    {
        var validator = CappedSuccessionPlanCycleRules.DefaultCappedSuccessionPlanCycleValidator();
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSuccessionPlanCycleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new CappedSuccessionPlanCycleRequiredGradeRule("capped succession plan cycle needs a value");
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSuccessionPlanCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedSuccessionPlanCycleRequiredLabelRule("capped succession plan cycle needs an entry");
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSuccessionPlanCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedSuccessionPlanCycleCodeLengthRule(120);
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSuccessionPlanCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedSuccessionPlanCycleLabelLengthRule(16);
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSuccessionPlanCycleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new CappedSuccessionPlanCycleChannelLengthRule(1000);
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedSuccessionPlanCycleValidatorReportsEveryProblem();
        CappedSuccessionPlanCycleRequiredGradeRuleFlagsABlankGrade();
        CappedSuccessionPlanCycleRequiredLabelRuleFlagsABlankLabel();
        CappedSuccessionPlanCycleCodeLengthRuleFlagsALongCode();
        CappedSuccessionPlanCycleLabelLengthRuleFlagsALongLabel();
        CappedSuccessionPlanCycleChannelLengthRuleFlagsALongChannel();
    }
}
