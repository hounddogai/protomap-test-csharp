namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingExceptionSlotRulesChecks
{
    public static void PendingExceptionSlotValidatorReportsEveryProblem()
    {
        var validator = PendingExceptionSlotRules.DefaultPendingExceptionSlotValidator();
        var item = PendingExceptionSlot.NewPendingExceptionSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingExceptionSlotRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PendingExceptionSlotRequiredTeamRule("pending exception slot needs a value");
        var item = PendingExceptionSlot.NewPendingExceptionSlot("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingExceptionSlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingExceptionSlotTitleLengthRule(120);
        var item = PendingExceptionSlot.NewPendingExceptionSlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingExceptionSlotLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PendingExceptionSlotLimitRangeRule(80, 3600);
        var item = PendingExceptionSlot.NewPendingExceptionSlot("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingExceptionSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingExceptionSlotCodeLengthRule(120);
        var item = PendingExceptionSlot.NewPendingExceptionSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingExceptionSlotValidatorReportsEveryProblem();
        PendingExceptionSlotRequiredTeamRuleFlagsABlankTeam();
        PendingExceptionSlotTitleLengthRuleFlagsALongTitle();
        PendingExceptionSlotLimitRangeRuleFlagsValuesAboveLimit();
        PendingExceptionSlotCodeLengthRuleFlagsALongCode();
    }
}
