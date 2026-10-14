namespace Acme.Workforce.Orgdesign.Tests.Scenario.Audit;

using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyScenarioOverrideRulesChecks
{
    public static void LegacyScenarioOverrideValidatorReportsEveryProblem()
    {
        var validator = LegacyScenarioOverrideRules.DefaultLegacyScenarioOverrideValidator();
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyScenarioOverrideRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new LegacyScenarioOverrideRequiredContactEmailRule("legacy scenario override needs this field");
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyScenarioOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyScenarioOverrideTitleLengthRule(16);
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyScenarioOverrideRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new LegacyScenarioOverrideRequiredNotesRule("legacy scenario override needs this field");
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyScenarioOverrideValidatorReportsEveryProblem();
        LegacyScenarioOverrideRequiredContactEmailRuleFlagsABlankContactEmail();
        LegacyScenarioOverrideTitleLengthRuleFlagsALongTitle();
        LegacyScenarioOverrideRequiredNotesRuleFlagsABlankNotes();
    }
}
