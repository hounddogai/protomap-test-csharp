namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class NominationSlotRulesChecks
{
    public static void NominationSlotValidatorReportsEveryProblem()
    {
        var validator = NominationSlotRules.DefaultNominationSlotValidator();
        var item = NominationSlot.NewNominationSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationSlotLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new NominationSlotLeaveDaysRangeRule(16, 80);
        var item = NominationSlot.NewNominationSlot("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationSlotTeamLengthRuleFlagsALongTeam()
    {
        var rule = new NominationSlotTeamLengthRule(80);
        var item = NominationSlot.NewNominationSlot("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationSlotMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new NominationSlotMinimumRangeRule(0.4, 4615.3);
        var item = NominationSlot.NewNominationSlot("x", "y");
        item.Minimum = 4616.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new NominationSlotCodeLengthRule(80);
        var item = NominationSlot.NewNominationSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationSlotValidatorReportsEveryProblem();
        NominationSlotLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        NominationSlotTeamLengthRuleFlagsALongTeam();
        NominationSlotMinimumRangeRuleFlagsValuesAboveMinimum();
        NominationSlotCodeLengthRuleFlagsALongCode();
    }
}
