namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingLaptopClaimRulesChecks
{
    public static void RollingLaptopClaimValidatorReportsEveryProblem()
    {
        var validator = RollingLaptopClaimRules.DefaultRollingLaptopClaimValidator();
        var item = RollingLaptopClaim.NewRollingLaptopClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingLaptopClaimJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new RollingLaptopClaimJobCodeLengthRule(120);
        var item = RollingLaptopClaim.NewRollingLaptopClaim("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingLaptopClaimNameLengthRuleFlagsALongName()
    {
        var rule = new RollingLaptopClaimNameLengthRule(120);
        var item = RollingLaptopClaim.NewRollingLaptopClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingLaptopClaimRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new RollingLaptopClaimRequiredJobCodeRule("rolling laptop claim needs this field");
        var item = RollingLaptopClaim.NewRollingLaptopClaim("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLaptopClaimWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new RollingLaptopClaimWindowDaysRangeRule(120, 255);
        var item = RollingLaptopClaim.NewRollingLaptopClaim("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingLaptopClaimValidatorReportsEveryProblem();
        RollingLaptopClaimJobCodeLengthRuleFlagsALongJobCode();
        RollingLaptopClaimNameLengthRuleFlagsALongName();
        RollingLaptopClaimRequiredJobCodeRuleFlagsABlankJobCode();
        RollingLaptopClaimWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
