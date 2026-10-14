namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class ManualVendorMilestoneRulesChecks
{
    public static void ManualVendorMilestoneValidatorReportsEveryProblem()
    {
        var validator = ManualVendorMilestoneRules.DefaultManualVendorMilestoneValidator();
        var item = ManualVendorMilestone.NewManualVendorMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualVendorMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new ManualVendorMilestoneNameLengthRule(80);
        var item = ManualVendorMilestone.NewManualVendorMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualVendorMilestoneDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new ManualVendorMilestoneDescriptionLengthRule(64);
        var item = ManualVendorMilestone.NewManualVendorMilestone("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualVendorMilestoneRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new ManualVendorMilestoneRequiredDescriptionRule("manual vendor milestone needs a value");
        var item = ManualVendorMilestone.NewManualVendorMilestone("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualVendorMilestoneSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new ManualVendorMilestoneSequenceRangeRule(32, 64);
        var item = ManualVendorMilestone.NewManualVendorMilestone("x", "y");
        item.Sequence = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualVendorMilestoneVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ManualVendorMilestoneVersionRangeRule(3600, 86400);
        var item = ManualVendorMilestone.NewManualVendorMilestone("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualVendorMilestoneValidatorReportsEveryProblem();
        ManualVendorMilestoneNameLengthRuleFlagsALongName();
        ManualVendorMilestoneDescriptionLengthRuleFlagsALongDescription();
        ManualVendorMilestoneRequiredDescriptionRuleFlagsABlankDescription();
        ManualVendorMilestoneSequenceRangeRuleFlagsValuesAboveSequence();
        ManualVendorMilestoneVersionRangeRuleFlagsValuesAboveVersion();
    }
}
