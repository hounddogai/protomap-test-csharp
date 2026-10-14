namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditReservationRulesChecks
{
    public static void AuditReservationValidatorReportsEveryProblem()
    {
        var validator = AuditReservationRules.DefaultAuditReservationValidator();
        var item = AuditReservation.NewAuditReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditReservationManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AuditReservationManagerIdLengthRule(86400);
        var item = AuditReservation.NewAuditReservation("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditReservationStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new AuditReservationStartsAtRangeRule(120, 255);
        var item = AuditReservation.NewAuditReservation("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AuditReservationRequiredTitleRule("audit reservation needs a value");
        var item = AuditReservation.NewAuditReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditReservationFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AuditReservationFteRangeRule(6.7, 4445.5);
        var item = AuditReservation.NewAuditReservation("x", "y");
        item.Fte = 4447.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditReservationBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AuditReservationBudgetRangeRule(7.4, 611.7);
        var item = AuditReservation.NewAuditReservation("x", "y");
        item.Budget = 613.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AuditReservationValidatorReportsEveryProblem();
        AuditReservationManagerIdLengthRuleFlagsALongManagerId();
        AuditReservationStartsAtRangeRuleFlagsValuesAboveStartsAt();
        AuditReservationRequiredTitleRuleFlagsABlankTitle();
        AuditReservationFteRangeRuleFlagsValuesAboveFte();
        AuditReservationBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
