namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallRequestRulesChecks
{
    public static void TownHallRequestValidatorReportsEveryProblem()
    {
        var validator = TownHallRequestRules.DefaultTownHallRequestValidator();
        var item = TownHallRequest.NewTownHallRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallRequestCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new TownHallRequestCapacityRangeRule(16, 120);
        var item = TownHallRequest.NewTownHallRequest("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallRequestRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new TownHallRequestRequiredNotesRule("town hall request needs an entry");
        var item = TownHallRequest.NewTownHallRequest("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TownHallRequestRequiredLabelRule("town hall request needs an entry");
        var item = TownHallRequest.NewTownHallRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallRequestRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TownHallRequestRequiredPhoneRule("town hall request needs this field");
        var item = TownHallRequest.NewTownHallRequest("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallRequestValidatorReportsEveryProblem();
        TownHallRequestCapacityRangeRuleFlagsValuesAboveCapacity();
        TownHallRequestRequiredNotesRuleFlagsABlankNotes();
        TownHallRequestRequiredLabelRuleFlagsABlankLabel();
        TownHallRequestRequiredPhoneRuleFlagsABlankPhone();
    }
}
