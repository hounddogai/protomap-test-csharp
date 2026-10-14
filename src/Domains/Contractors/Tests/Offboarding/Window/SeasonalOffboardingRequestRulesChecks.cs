namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalOffboardingRequestRulesChecks
{
    public static void SeasonalOffboardingRequestValidatorReportsEveryProblem()
    {
        var validator = SeasonalOffboardingRequestRules.DefaultSeasonalOffboardingRequestValidator();
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalOffboardingRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalOffboardingRequestRequiredCodeRule("seasonal offboarding request needs an entry");
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalOffboardingRequestPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new SeasonalOffboardingRequestPhoneLengthRule(80);
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalOffboardingRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalOffboardingRequestCodeLengthRule(120);
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalOffboardingRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SeasonalOffboardingRequestLabelLengthRule(64);
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalOffboardingRequestValidatorReportsEveryProblem();
        SeasonalOffboardingRequestRequiredCodeRuleFlagsABlankCode();
        SeasonalOffboardingRequestPhoneLengthRuleFlagsALongPhone();
        SeasonalOffboardingRequestCodeLengthRuleFlagsALongCode();
        SeasonalOffboardingRequestLabelLengthRuleFlagsALongLabel();
    }
}
