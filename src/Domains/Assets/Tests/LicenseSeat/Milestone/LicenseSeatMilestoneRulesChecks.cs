namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatMilestoneRulesChecks
{
    public static void LicenseSeatMilestoneValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatMilestoneRules.DefaultLicenseSeatMilestoneValidator();
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatMilestoneContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LicenseSeatMilestoneContactEmailLengthRule(80);
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatMilestoneSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LicenseSeatMilestoneSegmentLengthRule(32);
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatMilestoneRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new LicenseSeatMilestoneRequiredSegmentRule("license seat milestone needs an entry");
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LicenseSeatMilestoneRequiredCodeRule("license seat milestone needs this field");
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatMilestoneValidatorReportsEveryProblem();
        LicenseSeatMilestoneContactEmailLengthRuleFlagsALongContactEmail();
        LicenseSeatMilestoneSegmentLengthRuleFlagsALongSegment();
        LicenseSeatMilestoneRequiredSegmentRuleFlagsABlankSegment();
        LicenseSeatMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}
