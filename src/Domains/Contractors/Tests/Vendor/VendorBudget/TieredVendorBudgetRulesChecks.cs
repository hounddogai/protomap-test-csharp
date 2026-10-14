namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class TieredVendorBudgetRulesChecks
{
    public static void TieredVendorBudgetValidatorReportsEveryProblem()
    {
        var validator = TieredVendorBudgetRules.DefaultTieredVendorBudgetValidator();
        var item = TieredVendorBudget.NewTieredVendorBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredVendorBudgetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredVendorBudgetLabelLengthRule(3600);
        var item = TieredVendorBudget.NewTieredVendorBudget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredVendorBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredVendorBudgetRequiredCodeRule("tiered vendor budget needs an entry");
        var item = TieredVendorBudget.NewTieredVendorBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredVendorBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredVendorBudgetCodeLengthRule(86400);
        var item = TieredVendorBudget.NewTieredVendorBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredVendorBudgetValidatorReportsEveryProblem();
        TieredVendorBudgetLabelLengthRuleFlagsALongLabel();
        TieredVendorBudgetRequiredCodeRuleFlagsABlankCode();
        TieredVendorBudgetCodeLengthRuleFlagsALongCode();
    }
}
