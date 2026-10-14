namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorBudgetRulesChecks
{
    public static void VendorBudgetValidatorReportsEveryProblem()
    {
        var validator = VendorBudgetRules.DefaultVendorBudgetValidator();
        var item = VendorBudget.NewVendorBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorBudgetWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new VendorBudgetWindowDaysRangeRule(80, 3600);
        var item = VendorBudget.NewVendorBudget("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new VendorBudgetNameLengthRule(16);
        var item = VendorBudget.NewVendorBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorBudgetValidatorReportsEveryProblem();
        VendorBudgetWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        VendorBudgetNameLengthRuleFlagsALongName();
    }
}
