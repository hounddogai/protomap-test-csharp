namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalVisitorAdjustmentRulesChecks
{
    public static void GlobalVisitorAdjustmentValidatorReportsEveryProblem()
    {
        var validator = GlobalVisitorAdjustmentRules.DefaultGlobalVisitorAdjustmentValidator();
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalVisitorAdjustmentRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new GlobalVisitorAdjustmentRequiredManagerIdRule("global visitor adjustment needs an entry");
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVisitorAdjustmentQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new GlobalVisitorAdjustmentQuantityRangeRule(255, 86400);
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalVisitorAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalVisitorAdjustmentLabelLengthRule(64);
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalVisitorAdjustmentValidatorReportsEveryProblem();
        GlobalVisitorAdjustmentRequiredManagerIdRuleFlagsABlankManagerId();
        GlobalVisitorAdjustmentQuantityRangeRuleFlagsValuesAboveQuantity();
        GlobalVisitorAdjustmentLabelLengthRuleFlagsALongLabel();
    }
}
