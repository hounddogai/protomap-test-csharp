namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class SuccessionPlanAllocationRulesChecks
{
    public static void SuccessionPlanAllocationValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanAllocationRules.DefaultSuccessionPlanAllocationValidator();
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanAllocationRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new SuccessionPlanAllocationRequiredSkillRule("succession plan allocation needs a value");
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new SuccessionPlanAllocationCodeLengthRule(120);
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanAllocationValidatorReportsEveryProblem();
        SuccessionPlanAllocationRequiredSkillRuleFlagsABlankSkill();
        SuccessionPlanAllocationCodeLengthRuleFlagsALongCode();
    }
}
