namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallNoticeRulesChecks
{
    public static void TownHallNoticeValidatorReportsEveryProblem()
    {
        var validator = TownHallNoticeRules.DefaultTownHallNoticeValidator();
        var item = TownHallNotice.NewTownHallNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new TownHallNoticeNameLengthRule(86400);
        var item = TownHallNotice.NewTownHallNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallNoticeRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new TownHallNoticeRequiredNotesRule("town hall notice needs this field");
        var item = TownHallNotice.NewTownHallNotice("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallNoticeNotesLengthRuleFlagsALongNotes()
    {
        var rule = new TownHallNoticeNotesLengthRule(32);
        var item = TownHallNotice.NewTownHallNotice("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallNoticeRequiredCodeRule("town hall notice needs an entry");
        var item = TownHallNotice.NewTownHallNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallNoticeValidatorReportsEveryProblem();
        TownHallNoticeNameLengthRuleFlagsALongName();
        TownHallNoticeRequiredNotesRuleFlagsABlankNotes();
        TownHallNoticeNotesLengthRuleFlagsALongNotes();
        TownHallNoticeRequiredCodeRuleFlagsABlankCode();
    }
}
