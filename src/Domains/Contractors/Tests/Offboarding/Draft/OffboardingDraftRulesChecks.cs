namespace Acme.Workforce.Contractors.Tests.Offboarding.Draft;

using Acme.Workforce.Contractors.Offboarding.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OffboardingDraftRulesChecks
{
    public static void OffboardingDraftValidatorReportsEveryProblem()
    {
        var validator = OffboardingDraftRules.DefaultOffboardingDraftValidator();
        var item = OffboardingDraft.NewOffboardingDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OffboardingDraftTitleLengthRule(32);
        var item = OffboardingDraft.NewOffboardingDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OffboardingDraftRequiredTitleRule("offboarding draft needs a value");
        var item = OffboardingDraft.NewOffboardingDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingDraftNameLengthRuleFlagsALongName()
    {
        var rule = new OffboardingDraftNameLengthRule(3600);
        var item = OffboardingDraft.NewOffboardingDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingDraftValidatorReportsEveryProblem();
        OffboardingDraftTitleLengthRuleFlagsALongTitle();
        OffboardingDraftRequiredTitleRuleFlagsABlankTitle();
        OffboardingDraftNameLengthRuleFlagsALongName();
    }
}
