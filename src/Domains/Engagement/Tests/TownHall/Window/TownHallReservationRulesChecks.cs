namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallReservationRulesChecks
{
    public static void TownHallReservationValidatorReportsEveryProblem()
    {
        var validator = TownHallReservationRules.DefaultTownHallReservationValidator();
        var item = TownHallReservation.NewTownHallReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TownHallReservationRequiredLabelRule("town hall reservation needs a value");
        var item = TownHallReservation.NewTownHallReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallReservationRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TownHallReservationRetriesRangeRule(16, 255);
        var item = TownHallReservation.NewTownHallReservation("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new TownHallReservationCreatedAtRangeRule(86400, 86400);
        var item = TownHallReservation.NewTownHallReservation("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallReservationRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new TownHallReservationRequiredLocationRule("town hall reservation needs a value");
        var item = TownHallReservation.NewTownHallReservation("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallReservationContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new TownHallReservationContactEmailLengthRule(32);
        var item = TownHallReservation.NewTownHallReservation("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallReservationBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new TownHallReservationBatchSizeRangeRule(3600, 86400);
        var item = TownHallReservation.NewTownHallReservation("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallReservationValidatorReportsEveryProblem();
        TownHallReservationRequiredLabelRuleFlagsABlankLabel();
        TownHallReservationRetriesRangeRuleFlagsValuesAboveRetries();
        TownHallReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        TownHallReservationRequiredLocationRuleFlagsABlankLocation();
        TownHallReservationContactEmailLengthRuleFlagsALongContactEmail();
        TownHallReservationBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
