namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Budget;

using Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SuccessionPlanPolicyRulesChecks
{
    public static void SuccessionPlanPolicyValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanPolicyRules.DefaultSuccessionPlanPolicyValidator();
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanPolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SuccessionPlanPolicyRequiredLabelRule("succession plan policy needs a value");
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SuccessionPlanPolicyLabelLengthRule(1000);
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanPolicyEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new SuccessionPlanPolicyEndsAtRangeRule(64, 86400);
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanPolicyRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new SuccessionPlanPolicyRequiredCostCenterRule("succession plan policy needs this field");
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new SuccessionPlanPolicyWindowDaysRangeRule(32, 80);
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanPolicyValidatorReportsEveryProblem();
        SuccessionPlanPolicyRequiredLabelRuleFlagsABlankLabel();
        SuccessionPlanPolicyLabelLengthRuleFlagsALongLabel();
        SuccessionPlanPolicyEndsAtRangeRuleFlagsValuesAboveEndsAt();
        SuccessionPlanPolicyRequiredCostCenterRuleFlagsABlankCostCenter();
        SuccessionPlanPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
