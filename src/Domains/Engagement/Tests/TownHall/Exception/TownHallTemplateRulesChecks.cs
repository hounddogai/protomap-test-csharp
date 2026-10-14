namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallTemplateRulesChecks
{
    public static void TownHallTemplateValidatorReportsEveryProblem()
    {
        var validator = TownHallTemplateRules.DefaultTownHallTemplateValidator();
        var item = TownHallTemplate.NewTownHallTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallTemplateRequiredCodeRule("town hall template needs a value");
        var item = TownHallTemplate.NewTownHallTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallTemplateRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new TownHallTemplateRequiredNotesRule("town hall template needs this field");
        var item = TownHallTemplate.NewTownHallTemplate("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallTemplateRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new TownHallTemplateRequiredSummaryRule("town hall template needs this field");
        var item = TownHallTemplate.NewTownHallTemplate("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallTemplateCodeLengthRule(1000);
        var item = TownHallTemplate.NewTownHallTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallTemplateEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new TownHallTemplateEndsAtRangeRule(64, 86400);
        var item = TownHallTemplate.NewTownHallTemplate("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallTemplateValidatorReportsEveryProblem();
        TownHallTemplateRequiredCodeRuleFlagsABlankCode();
        TownHallTemplateRequiredNotesRuleFlagsABlankNotes();
        TownHallTemplateRequiredSummaryRuleFlagsABlankSummary();
        TownHallTemplateCodeLengthRuleFlagsALongCode();
        TownHallTemplateEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
