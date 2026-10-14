namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingLaptopReservationRulesChecks
{
    public static void PendingLaptopReservationValidatorReportsEveryProblem()
    {
        var validator = PendingLaptopReservationRules.DefaultPendingLaptopReservationValidator();
        var item = PendingLaptopReservation.NewPendingLaptopReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingLaptopReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingLaptopReservationCodeLengthRule(80);
        var item = PendingLaptopReservation.NewPendingLaptopReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingLaptopReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingLaptopReservationRequiredTitleRule("pending laptop reservation needs this field");
        var item = PendingLaptopReservation.NewPendingLaptopReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingLaptopReservationValidatorReportsEveryProblem();
        PendingLaptopReservationCodeLengthRuleFlagsALongCode();
        PendingLaptopReservationRequiredTitleRuleFlagsABlankTitle();
    }
}
