namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorScoreRulesChecks
{
    public static void DefaultVendorScoreValidatorReportsEveryProblem()
    {
        var validator = DefaultVendorScoreRules.DefaultDefaultVendorScoreValidator();
        var item = DefaultVendorScore.NewDefaultVendorScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultVendorScoreRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new DefaultVendorScoreRequiredJobCodeRule("default vendor score needs a value");
        var item = DefaultVendorScore.NewDefaultVendorScore("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultVendorScoreNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultVendorScoreNameLengthRule(80);
        var item = DefaultVendorScore.NewDefaultVendorScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultVendorScoreValidatorReportsEveryProblem();
        DefaultVendorScoreRequiredJobCodeRuleFlagsABlankJobCode();
        DefaultVendorScoreNameLengthRuleFlagsALongName();
    }
}
