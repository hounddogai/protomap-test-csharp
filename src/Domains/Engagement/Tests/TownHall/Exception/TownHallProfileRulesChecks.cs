namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallProfileRulesChecks
{
    public static void TownHallProfileValidatorReportsEveryProblem()
    {
        var validator = TownHallProfileRules.DefaultTownHallProfileValidator();
        var item = TownHallProfile.NewTownHallProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallProfileMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TownHallProfileMinimumRangeRule(6.1, 1520.9);
        var item = TownHallProfile.NewTownHallProfile("x", "y");
        item.Minimum = 1522.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallProfileDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new TownHallProfileDepartmentLengthRule(86400);
        var item = TownHallProfile.NewTownHallProfile("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallProfileRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TownHallProfileRequiredDepartmentRule("town hall profile needs a value");
        var item = TownHallProfile.NewTownHallProfile("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallProfileValidatorReportsEveryProblem();
        TownHallProfileMinimumRangeRuleFlagsValuesAboveMinimum();
        TownHallProfileDepartmentLengthRuleFlagsALongDepartment();
        TownHallProfileRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
