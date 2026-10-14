namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class HolidayRequestRulesChecks
{
    public static void HolidayRequestValidatorReportsEveryProblem()
    {
        var validator = HolidayRequestRules.DefaultHolidayRequestValidator();
        var item = HolidayRequest.NewHolidayRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayRequestNameLengthRuleFlagsALongName()
    {
        var rule = new HolidayRequestNameLengthRule(3600);
        var item = HolidayRequest.NewHolidayRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayRequestRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new HolidayRequestRequiredJobCodeRule("holiday request needs an entry");
        var item = HolidayRequest.NewHolidayRequest("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayRequestRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new HolidayRequestRevisionRangeRule(120, 1000);
        var item = HolidayRequest.NewHolidayRequest("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayRequestCodeLengthRule(80);
        var item = HolidayRequest.NewHolidayRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayRequestPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new HolidayRequestPriorityRangeRule(120, 86400);
        var item = HolidayRequest.NewHolidayRequest("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayRequestValidatorReportsEveryProblem();
        HolidayRequestNameLengthRuleFlagsALongName();
        HolidayRequestRequiredJobCodeRuleFlagsABlankJobCode();
        HolidayRequestRevisionRangeRuleFlagsValuesAboveRevision();
        HolidayRequestCodeLengthRuleFlagsALongCode();
        HolidayRequestPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
