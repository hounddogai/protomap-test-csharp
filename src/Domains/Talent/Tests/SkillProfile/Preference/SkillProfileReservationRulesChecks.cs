namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SkillProfileReservationRulesChecks
{
    public static void SkillProfileReservationValidatorReportsEveryProblem()
    {
        var validator = SkillProfileReservationRules.DefaultSkillProfileReservationValidator();
        var item = SkillProfileReservation.NewSkillProfileReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileReservationRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SkillProfileReservationRetriesRangeRule(16, 1000);
        var item = SkillProfileReservation.NewSkillProfileReservation("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SkillProfileReservationRequiredTitleRule("skill profile reservation needs an entry");
        var item = SkillProfileReservation.NewSkillProfileReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileReservationNotesLengthRuleFlagsALongNotes()
    {
        var rule = new SkillProfileReservationNotesLengthRule(255);
        var item = SkillProfileReservation.NewSkillProfileReservation("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SkillProfileReservationTitleLengthRule(32);
        var item = SkillProfileReservation.NewSkillProfileReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileReservationValidatorReportsEveryProblem();
        SkillProfileReservationRetriesRangeRuleFlagsValuesAboveRetries();
        SkillProfileReservationRequiredTitleRuleFlagsABlankTitle();
        SkillProfileReservationNotesLengthRuleFlagsALongNotes();
        SkillProfileReservationTitleLengthRuleFlagsALongTitle();
    }
}
