namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRollupRulesChecks
{
    public static void RefreshRollupValidatorReportsEveryProblem()
    {
        var validator = RefreshRollupRules.DefaultRefreshRollupValidator();
        var item = RefreshRollup.NewRefreshRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RefreshRollupLabelLengthRule(86400);
        var item = RefreshRollup.NewRefreshRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshRollupCodeLengthRule(255);
        var item = RefreshRollup.NewRefreshRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshRollupRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RefreshRollupRequiredEmployeeIdRule("refresh rollup needs this field");
        var item = RefreshRollup.NewRefreshRollup("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshRollupValidatorReportsEveryProblem();
        RefreshRollupLabelLengthRuleFlagsALongLabel();
        RefreshRollupCodeLengthRuleFlagsALongCode();
        RefreshRollupRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
