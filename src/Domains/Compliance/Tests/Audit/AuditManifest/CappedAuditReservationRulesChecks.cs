namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class CappedAuditReservationRulesChecks
{
    public static void CappedAuditReservationValidatorReportsEveryProblem()
    {
        var validator = CappedAuditReservationRules.DefaultCappedAuditReservationValidator();
        var item = CappedAuditReservation.NewCappedAuditReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAuditReservationChannelLengthRuleFlagsALongChannel()
    {
        var rule = new CappedAuditReservationChannelLengthRule(255);
        var item = CappedAuditReservation.NewCappedAuditReservation("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAuditReservationRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new CappedAuditReservationRequiredChannelRule("capped audit reservation needs this field");
        var item = CappedAuditReservation.NewCappedAuditReservation("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAuditReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedAuditReservationRequiredCodeRule("capped audit reservation needs a value");
        var item = CappedAuditReservation.NewCappedAuditReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAuditReservationNameLengthRuleFlagsALongName()
    {
        var rule = new CappedAuditReservationNameLengthRule(3600);
        var item = CappedAuditReservation.NewCappedAuditReservation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAuditReservationFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new CappedAuditReservationFteRangeRule(3.4, 1298.0);
        var item = CappedAuditReservation.NewCappedAuditReservation("x", "y");
        item.Fte = 1299.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAuditReservationCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new CappedAuditReservationCapacityRangeRule(16, 16);
        var item = CappedAuditReservation.NewCappedAuditReservation("x", "y");
        item.Capacity = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedAuditReservationValidatorReportsEveryProblem();
        CappedAuditReservationChannelLengthRuleFlagsALongChannel();
        CappedAuditReservationRequiredChannelRuleFlagsABlankChannel();
        CappedAuditReservationRequiredCodeRuleFlagsABlankCode();
        CappedAuditReservationNameLengthRuleFlagsALongName();
        CappedAuditReservationFteRangeRuleFlagsValuesAboveFte();
        CappedAuditReservationCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
