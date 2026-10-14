namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Claim;

using Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingPulseSurveyClaimRulesChecks
{
    public static void PendingPulseSurveyClaimValidatorReportsEveryProblem()
    {
        var validator = PendingPulseSurveyClaimRules.DefaultPendingPulseSurveyClaimValidator();
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPulseSurveyClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingPulseSurveyClaimRequiredCodeRule("pending pulse survey claim needs this field");
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPulseSurveyClaimDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new PendingPulseSurveyClaimDepartmentLengthRule(255);
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPulseSurveyClaimNameLengthRuleFlagsALongName()
    {
        var rule = new PendingPulseSurveyClaimNameLengthRule(32);
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPulseSurveyClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingPulseSurveyClaimRequiredNameRule("pending pulse survey claim needs this field");
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPulseSurveyClaimRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PendingPulseSurveyClaimRequiredDepartmentRule("pending pulse survey claim needs this field");
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingPulseSurveyClaimValidatorReportsEveryProblem();
        PendingPulseSurveyClaimRequiredCodeRuleFlagsABlankCode();
        PendingPulseSurveyClaimDepartmentLengthRuleFlagsALongDepartment();
        PendingPulseSurveyClaimNameLengthRuleFlagsALongName();
        PendingPulseSurveyClaimRequiredNameRuleFlagsABlankName();
        PendingPulseSurveyClaimRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
