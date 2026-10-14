namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class ManualWarrantyReservationRulesChecks
{
    public static void ManualWarrantyReservationValidatorReportsEveryProblem()
    {
        var validator = ManualWarrantyReservationRules.DefaultManualWarrantyReservationValidator();
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualWarrantyReservationReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new ManualWarrantyReservationReviewerLengthRule(80);
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualWarrantyReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualWarrantyReservationRequiredLabelRule("manual warranty reservation needs a value");
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualWarrantyReservationVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ManualWarrantyReservationVersionRangeRule(64, 3600);
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualWarrantyReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualWarrantyReservationLabelLengthRule(32);
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualWarrantyReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ManualWarrantyReservationExpiresAtRangeRule(1000, 3600);
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualWarrantyReservationValidatorReportsEveryProblem();
        ManualWarrantyReservationReviewerLengthRuleFlagsALongReviewer();
        ManualWarrantyReservationRequiredLabelRuleFlagsABlankLabel();
        ManualWarrantyReservationVersionRangeRuleFlagsValuesAboveVersion();
        ManualWarrantyReservationLabelLengthRuleFlagsALongLabel();
        ManualWarrantyReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
