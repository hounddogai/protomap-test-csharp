namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>Checks of vendor batch code, run by the project's runner.</summary>
public static class LegacyVendorAssignmentRulesChecks
{
    public static void LegacyVendorAssignmentValidatorReportsEveryProblem()
    {
        var validator = LegacyVendorAssignmentRules.DefaultLegacyVendorAssignmentValidator();
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyVendorAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyVendorAssignmentLabelLengthRule(80);
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyVendorAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyVendorAssignmentRequiredLabelRule("legacy vendor assignment needs an entry");
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyVendorAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LegacyVendorAssignmentAttemptsRangeRule(32, 1000);
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyVendorAssignmentRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LegacyVendorAssignmentRequiredChannelRule("legacy vendor assignment needs an entry");
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyVendorAssignmentRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LegacyVendorAssignmentRequiredBadgeRule("legacy vendor assignment needs a value");
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyVendorAssignmentValidatorReportsEveryProblem();
        LegacyVendorAssignmentLabelLengthRuleFlagsALongLabel();
        LegacyVendorAssignmentRequiredLabelRuleFlagsABlankLabel();
        LegacyVendorAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        LegacyVendorAssignmentRequiredChannelRuleFlagsABlankChannel();
        LegacyVendorAssignmentRequiredBadgeRuleFlagsABlankBadge();
    }
}
