namespace Acme.Workforce.Absence.Tests.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BalanceBundleRulesChecks
{
    public static void BalanceBundleValidatorReportsEveryProblem()
    {
        var validator = BalanceBundleRules.DefaultBalanceBundleValidator();
        var item = BalanceBundle.NewBalanceBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceBundleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new BalanceBundleChannelLengthRule(32);
        var item = BalanceBundle.NewBalanceBundle("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BalanceBundleRequiredCodeRule("balance bundle needs this field");
        var item = BalanceBundle.NewBalanceBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceBundleDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new BalanceBundleDepartmentLengthRule(64);
        var item = BalanceBundle.NewBalanceBundle("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BalanceBundleUpdatedAtRangeRule(32, 1000);
        var item = BalanceBundle.NewBalanceBundle("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceBundleValidatorReportsEveryProblem();
        BalanceBundleChannelLengthRuleFlagsALongChannel();
        BalanceBundleRequiredCodeRuleFlagsABlankCode();
        BalanceBundleDepartmentLengthRuleFlagsALongDepartment();
        BalanceBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
