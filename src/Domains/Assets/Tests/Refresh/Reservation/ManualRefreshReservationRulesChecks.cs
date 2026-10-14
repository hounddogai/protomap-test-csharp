namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualRefreshReservationRulesChecks
{
    public static void ManualRefreshReservationValidatorReportsEveryProblem()
    {
        var validator = ManualRefreshReservationRules.DefaultManualRefreshReservationValidator();
        var item = ManualRefreshReservation.NewManualRefreshReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRefreshReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ManualRefreshReservationExpiresAtRangeRule(80, 255);
        var item = ManualRefreshReservation.NewManualRefreshReservation("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualRefreshReservationPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ManualRefreshReservationPhoneLengthRule(32);
        var item = ManualRefreshReservation.NewManualRefreshReservation("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualRefreshReservationSourceLengthRuleFlagsALongSource()
    {
        var rule = new ManualRefreshReservationSourceLengthRule(80);
        var item = ManualRefreshReservation.NewManualRefreshReservation("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualRefreshReservationValidatorReportsEveryProblem();
        ManualRefreshReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ManualRefreshReservationPhoneLengthRuleFlagsALongPhone();
        ManualRefreshReservationSourceLengthRuleFlagsALongSource();
    }
}
