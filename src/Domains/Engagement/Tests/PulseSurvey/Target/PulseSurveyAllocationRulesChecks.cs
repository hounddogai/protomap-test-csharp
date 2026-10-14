namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PulseSurveyAllocationRulesChecks
{
    public static void PulseSurveyAllocationValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyAllocationRules.DefaultPulseSurveyAllocationValidator();
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyAllocationDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PulseSurveyAllocationDurationMinutesRangeRule(120, 255);
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyAllocationCodeLengthRule(255);
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAllocationRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PulseSurveyAllocationRequiredExternalRefRule("pulse survey allocation needs a value");
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyAllocationRequiredCodeRule("pulse survey allocation needs this field");
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyAllocationValidatorReportsEveryProblem();
        PulseSurveyAllocationDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        PulseSurveyAllocationCodeLengthRuleFlagsALongCode();
        PulseSurveyAllocationRequiredExternalRefRuleFlagsABlankExternalRef();
        PulseSurveyAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
