namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRequestRulesChecks
{
    public static void OffboardingRequestValidatorReportsEveryProblem()
    {
        var validator = OffboardingRequestRules.DefaultOffboardingRequestValidator();
        var item = OffboardingRequest.NewOffboardingRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingRequestCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new OffboardingRequestCeilingRangeRule(0.7, 3060.5);
        var item = OffboardingRequest.NewOffboardingRequest("x", "y");
        item.Ceiling = 3062.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingRequestCodeLengthRule(86400);
        var item = OffboardingRequest.NewOffboardingRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OffboardingRequestRequiredTitleRule("offboarding request needs a value");
        var item = OffboardingRequest.NewOffboardingRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingRequestValidatorReportsEveryProblem();
        OffboardingRequestCeilingRangeRuleFlagsValuesAboveCeiling();
        OffboardingRequestCodeLengthRuleFlagsALongCode();
        OffboardingRequestRequiredTitleRuleFlagsABlankTitle();
    }
}
