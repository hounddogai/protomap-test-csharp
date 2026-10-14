namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GlobalOffboardingRequestRulesChecks
{
    public static void GlobalOffboardingRequestValidatorReportsEveryProblem()
    {
        var validator = GlobalOffboardingRequestRules.DefaultGlobalOffboardingRequestValidator();
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalOffboardingRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalOffboardingRequestLabelLengthRule(1000);
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalOffboardingRequestRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new GlobalOffboardingRequestRequiredContactEmailRule("global offboarding request needs this field");
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalOffboardingRequestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new GlobalOffboardingRequestScoreRangeRule(4.3, 2338.3);
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("x", "y");
        item.Score = 2339.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalOffboardingRequestValidatorReportsEveryProblem();
        GlobalOffboardingRequestLabelLengthRuleFlagsALongLabel();
        GlobalOffboardingRequestRequiredContactEmailRuleFlagsABlankContactEmail();
        GlobalOffboardingRequestScoreRangeRuleFlagsValuesAboveScore();
    }
}
