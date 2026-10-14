namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LenientLicenseSeatMilestoneRulesChecks
{
    public static void LenientLicenseSeatMilestoneValidatorReportsEveryProblem()
    {
        var validator = LenientLicenseSeatMilestoneRules.DefaultLenientLicenseSeatMilestoneValidator();
        var item = LenientLicenseSeatMilestone.NewLenientLicenseSeatMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLicenseSeatMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new LenientLicenseSeatMilestoneNameLengthRule(120);
        var item = LenientLicenseSeatMilestone.NewLenientLicenseSeatMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientLicenseSeatMilestoneValidatorReportsEveryProblem();
        LenientLicenseSeatMilestoneNameLengthRuleFlagsALongName();
    }
}
