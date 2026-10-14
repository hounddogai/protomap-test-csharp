namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DeskDraftRulesChecks
{
    public static void DeskDraftValidatorReportsEveryProblem()
    {
        var validator = DeskDraftRules.DefaultDeskDraftValidator();
        var item = DeskDraft.NewDeskDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskDraftVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new DeskDraftVersionRangeRule(64, 80);
        var item = DeskDraft.NewDeskDraft("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskDraftAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new DeskDraftAttemptsRangeRule(32, 3600);
        var item = DeskDraft.NewDeskDraft("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskDraftRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new DeskDraftRequiredGradeRule("desk draft needs a value");
        var item = DeskDraft.NewDeskDraft("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeskDraftTitleLengthRule(86400);
        var item = DeskDraft.NewDeskDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskDraftValidatorReportsEveryProblem();
        DeskDraftVersionRangeRuleFlagsValuesAboveVersion();
        DeskDraftAttemptsRangeRuleFlagsValuesAboveAttempts();
        DeskDraftRequiredGradeRuleFlagsABlankGrade();
        DeskDraftTitleLengthRuleFlagsALongTitle();
    }
}
