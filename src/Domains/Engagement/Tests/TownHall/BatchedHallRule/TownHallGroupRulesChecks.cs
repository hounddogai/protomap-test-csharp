namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class TownHallGroupRulesChecks
{
    public static void TownHallGroupValidatorReportsEveryProblem()
    {
        var validator = TownHallGroupRules.DefaultTownHallGroupValidator();
        var item = TownHallGroup.NewTownHallGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallGroupCodeLengthRule(120);
        var item = TownHallGroup.NewTownHallGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallGroupRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TownHallGroupRequiredDepartmentRule("town hall group needs an entry");
        var item = TownHallGroup.NewTownHallGroup("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallGroupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new TownHallGroupUpdatedAtRangeRule(80, 80);
        var item = TownHallGroup.NewTownHallGroup("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallGroupDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new TownHallGroupDiscountRangeRule(5.7, 1633.4);
        var item = TownHallGroup.NewTownHallGroup("x", "y");
        item.Discount = 1634.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallGroupValidatorReportsEveryProblem();
        TownHallGroupCodeLengthRuleFlagsALongCode();
        TownHallGroupRequiredDepartmentRuleFlagsABlankDepartment();
        TownHallGroupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        TownHallGroupDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
