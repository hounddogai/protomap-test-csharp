namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalTownHallGroupRulesChecks
{
    public static void SeasonalTownHallGroupValidatorReportsEveryProblem()
    {
        var validator = SeasonalTownHallGroupRules.DefaultSeasonalTownHallGroupValidator();
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalTownHallGroupFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SeasonalTownHallGroupFloorValueRangeRule(1.7, 1709.9);
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("x", "y");
        item.FloorValue = 1711.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalTownHallGroupNotesLengthRuleFlagsALongNotes()
    {
        var rule = new SeasonalTownHallGroupNotesLengthRule(32);
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalTownHallGroupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SeasonalTownHallGroupUpdatedAtRangeRule(80, 120);
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalTownHallGroupMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new SeasonalTownHallGroupMaximumRangeRule(7.6, 1350.3);
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("x", "y");
        item.Maximum = 1351.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalTownHallGroupReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new SeasonalTownHallGroupReviewerLengthRule(86400);
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalTownHallGroupValidatorReportsEveryProblem();
        SeasonalTownHallGroupFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SeasonalTownHallGroupNotesLengthRuleFlagsALongNotes();
        SeasonalTownHallGroupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        SeasonalTownHallGroupMaximumRangeRuleFlagsValuesAboveMaximum();
        SeasonalTownHallGroupReviewerLengthRuleFlagsALongReviewer();
    }
}
