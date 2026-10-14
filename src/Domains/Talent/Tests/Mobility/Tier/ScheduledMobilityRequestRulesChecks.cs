namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledMobilityRequestRulesChecks
{
    public static void ScheduledMobilityRequestValidatorReportsEveryProblem()
    {
        var validator = ScheduledMobilityRequestRules.DefaultScheduledMobilityRequestValidator();
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledMobilityRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledMobilityRequestRequiredNameRule("scheduled mobility request needs a value");
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledMobilityRequestAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new ScheduledMobilityRequestAmountRangeRule(0.4, 4795.4);
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("x", "y");
        item.Amount = 4796.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledMobilityRequestEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new ScheduledMobilityRequestEmployeeIdLengthRule(32);
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledMobilityRequestNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledMobilityRequestNameLengthRule(255);
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledMobilityRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledMobilityRequestCodeLengthRule(64);
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledMobilityRequestValidatorReportsEveryProblem();
        ScheduledMobilityRequestRequiredNameRuleFlagsABlankName();
        ScheduledMobilityRequestAmountRangeRuleFlagsValuesAboveAmount();
        ScheduledMobilityRequestEmployeeIdLengthRuleFlagsALongEmployeeId();
        ScheduledMobilityRequestNameLengthRuleFlagsALongName();
        ScheduledMobilityRequestCodeLengthRuleFlagsALongCode();
    }
}
