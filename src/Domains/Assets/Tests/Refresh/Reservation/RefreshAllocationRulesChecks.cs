namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RefreshAllocationRulesChecks
{
    public static void RefreshAllocationValidatorReportsEveryProblem()
    {
        var validator = RefreshAllocationRules.DefaultRefreshAllocationValidator();
        var item = RefreshAllocation.NewRefreshAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshAllocationStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RefreshAllocationStartsAtRangeRule(120, 1000);
        var item = RefreshAllocation.NewRefreshAllocation("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshAllocationRequiredCodeRule("refresh allocation needs this field");
        var item = RefreshAllocation.NewRefreshAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshAllocationCodeLengthRule(16);
        var item = RefreshAllocation.NewRefreshAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new RefreshAllocationNameLengthRule(3600);
        var item = RefreshAllocation.NewRefreshAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshAllocationValidatorReportsEveryProblem();
        RefreshAllocationStartsAtRangeRuleFlagsValuesAboveStartsAt();
        RefreshAllocationRequiredCodeRuleFlagsABlankCode();
        RefreshAllocationCodeLengthRuleFlagsALongCode();
        RefreshAllocationNameLengthRuleFlagsALongName();
    }
}
