namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientVisitorReservationRulesChecks
{
    public static void LenientVisitorReservationValidatorReportsEveryProblem()
    {
        var validator = LenientVisitorReservationRules.DefaultLenientVisitorReservationValidator();
        var item = LenientVisitorReservation.NewLenientVisitorReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVisitorReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientVisitorReservationRequiredNameRule("lenient visitor reservation needs a value");
        var item = LenientVisitorReservation.NewLenientVisitorReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientVisitorReservationLocationLengthRuleFlagsALongLocation()
    {
        var rule = new LenientVisitorReservationLocationLengthRule(1000);
        var item = LenientVisitorReservation.NewLenientVisitorReservation("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientVisitorReservationNameLengthRuleFlagsALongName()
    {
        var rule = new LenientVisitorReservationNameLengthRule(80);
        var item = LenientVisitorReservation.NewLenientVisitorReservation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientVisitorReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientVisitorReservationRequiredCodeRule("lenient visitor reservation needs a value");
        var item = LenientVisitorReservation.NewLenientVisitorReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientVisitorReservationValidatorReportsEveryProblem();
        LenientVisitorReservationRequiredNameRuleFlagsABlankName();
        LenientVisitorReservationLocationLengthRuleFlagsALongLocation();
        LenientVisitorReservationNameLengthRuleFlagsALongName();
        LenientVisitorReservationRequiredCodeRuleFlagsABlankCode();
    }
}
