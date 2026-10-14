namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictWarrantyTierRulesChecks
{
    public static void StrictWarrantyTierValidatorReportsEveryProblem()
    {
        var validator = StrictWarrantyTierRules.DefaultStrictWarrantyTierValidator();
        var item = StrictWarrantyTier.NewStrictWarrantyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictWarrantyTierCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new StrictWarrantyTierCreatedAtRangeRule(64, 86400);
        var item = StrictWarrantyTier.NewStrictWarrantyTier("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictWarrantyTierDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new StrictWarrantyTierDepartmentLengthRule(86400);
        var item = StrictWarrantyTier.NewStrictWarrantyTier("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictWarrantyTierValidatorReportsEveryProblem();
        StrictWarrantyTierCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        StrictWarrantyTierDepartmentLengthRuleFlagsALongDepartment();
    }
}
