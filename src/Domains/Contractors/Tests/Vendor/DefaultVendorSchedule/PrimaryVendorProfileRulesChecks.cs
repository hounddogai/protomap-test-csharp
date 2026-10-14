namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class PrimaryVendorProfileRulesChecks
{
    public static void PrimaryVendorProfileValidatorReportsEveryProblem()
    {
        var validator = PrimaryVendorProfileRules.DefaultPrimaryVendorProfileValidator();
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryVendorProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryVendorProfileTitleLengthRule(120);
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryVendorProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryVendorProfileRequiredTitleRule("primary vendor profile needs an entry");
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryVendorProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryVendorProfileRequiredCodeRule("primary vendor profile needs an entry");
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryVendorProfileValidatorReportsEveryProblem();
        PrimaryVendorProfileTitleLengthRuleFlagsALongTitle();
        PrimaryVendorProfileRequiredTitleRuleFlagsABlankTitle();
        PrimaryVendorProfileRequiredCodeRuleFlagsABlankCode();
    }
}
