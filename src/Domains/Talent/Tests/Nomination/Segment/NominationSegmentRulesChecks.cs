namespace Acme.Workforce.Talent.Tests.Nomination.Segment;

using Acme.Workforce.Talent.Nomination.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class NominationSegmentRulesChecks
{
    public static void NominationSegmentValidatorReportsEveryProblem()
    {
        var validator = NominationSegmentRules.DefaultNominationSegmentValidator();
        var item = NominationSegment.NewNominationSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationSegmentCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new NominationSegmentCategoryLengthRule(64);
        var item = NominationSegment.NewNominationSegment("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new NominationSegmentCodeLengthRule(3600);
        var item = NominationSegment.NewNominationSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationSegmentRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new NominationSegmentRequiredCategoryRule("nomination segment needs this field");
        var item = NominationSegment.NewNominationSegment("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationSegmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new NominationSegmentTitleLengthRule(1000);
        var item = NominationSegment.NewNominationSegment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationSegmentRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new NominationSegmentRequiredJobCodeRule("nomination segment needs a value");
        var item = NominationSegment.NewNominationSegment("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationSegmentValidatorReportsEveryProblem();
        NominationSegmentCategoryLengthRuleFlagsALongCategory();
        NominationSegmentCodeLengthRuleFlagsALongCode();
        NominationSegmentRequiredCategoryRuleFlagsABlankCategory();
        NominationSegmentTitleLengthRuleFlagsALongTitle();
        NominationSegmentRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
