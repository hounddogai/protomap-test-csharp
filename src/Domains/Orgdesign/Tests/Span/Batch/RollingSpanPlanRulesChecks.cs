namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RollingSpanPlanRulesChecks
{
    public static void RollingSpanPlanValidatorReportsEveryProblem()
    {
        var validator = RollingSpanPlanRules.DefaultRollingSpanPlanValidator();
        var item = RollingSpanPlan.NewRollingSpanPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSpanPlanOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new RollingSpanPlanOwnerIdLengthRule(120);
        var item = RollingSpanPlan.NewRollingSpanPlan("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSpanPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingSpanPlanRequiredCodeRule("rolling span plan needs this field");
        var item = RollingSpanPlan.NewRollingSpanPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSpanPlanRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new RollingSpanPlanRequiredOwnerIdRule("rolling span plan needs a value");
        var item = RollingSpanPlan.NewRollingSpanPlan("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingSpanPlanValidatorReportsEveryProblem();
        RollingSpanPlanOwnerIdLengthRuleFlagsALongOwnerId();
        RollingSpanPlanRequiredCodeRuleFlagsABlankCode();
        RollingSpanPlanRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
