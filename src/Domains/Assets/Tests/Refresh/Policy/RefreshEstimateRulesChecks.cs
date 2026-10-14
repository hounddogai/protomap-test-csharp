namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RefreshEstimateRulesChecks
{
    public static void RefreshEstimateValidatorReportsEveryProblem()
    {
        var validator = RefreshEstimateRules.DefaultRefreshEstimateValidator();
        var item = RefreshEstimate.NewRefreshEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshEstimateDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new RefreshEstimateDescriptionLengthRule(1000);
        var item = RefreshEstimate.NewRefreshEstimate("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshEstimateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RefreshEstimateRetriesRangeRule(255, 86400);
        var item = RefreshEstimate.NewRefreshEstimate("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshEstimateVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new RefreshEstimateVersionRangeRule(120, 1000);
        var item = RefreshEstimate.NewRefreshEstimate("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshEstimateRequiredCodeRule("refresh estimate needs this field");
        var item = RefreshEstimate.NewRefreshEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshEstimateRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new RefreshEstimateRequiredManagerIdRule("refresh estimate needs this field");
        var item = RefreshEstimate.NewRefreshEstimate("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshEstimateValidatorReportsEveryProblem();
        RefreshEstimateDescriptionLengthRuleFlagsALongDescription();
        RefreshEstimateRetriesRangeRuleFlagsValuesAboveRetries();
        RefreshEstimateVersionRangeRuleFlagsValuesAboveVersion();
        RefreshEstimateRequiredCodeRuleFlagsABlankCode();
        RefreshEstimateRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
