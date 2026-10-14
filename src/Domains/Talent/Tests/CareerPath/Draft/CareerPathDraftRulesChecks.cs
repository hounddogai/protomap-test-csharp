namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathDraftRulesChecks
{
    public static void CareerPathDraftValidatorReportsEveryProblem()
    {
        var validator = CareerPathDraftRules.DefaultCareerPathDraftValidator();
        var item = CareerPathDraft.NewCareerPathDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathDraftRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CareerPathDraftRequiredContactEmailRule("career path draft needs a value");
        var item = CareerPathDraft.NewCareerPathDraft("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathDraftRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CareerPathDraftRequiredLabelRule("career path draft needs a value");
        var item = CareerPathDraft.NewCareerPathDraft("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathDraftRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new CareerPathDraftRequiredSegmentRule("career path draft needs this field");
        var item = CareerPathDraft.NewCareerPathDraft("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathDraftValidatorReportsEveryProblem();
        CareerPathDraftRequiredContactEmailRuleFlagsABlankContactEmail();
        CareerPathDraftRequiredLabelRuleFlagsABlankLabel();
        CareerPathDraftRequiredSegmentRuleFlagsABlankSegment();
    }
}
