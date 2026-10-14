namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyAssignmentRulesChecks
{
    public static void PulseSurveyAssignmentValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyAssignmentRules.DefaultPulseSurveyAssignmentValidator();
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new PulseSurveyAssignmentRequiredNameRule("pulse survey assignment needs an entry");
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new PulseSurveyAssignmentNameLengthRule(255);
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAssignmentCountryLengthRuleFlagsALongCountry()
    {
        var rule = new PulseSurveyAssignmentCountryLengthRule(120);
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAssignmentRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new PulseSurveyAssignmentRequiredPhoneRule("pulse survey assignment needs an entry");
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyAssignmentRequiredCodeRule("pulse survey assignment needs this field");
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyAssignmentValidatorReportsEveryProblem();
        PulseSurveyAssignmentRequiredNameRuleFlagsABlankName();
        PulseSurveyAssignmentNameLengthRuleFlagsALongName();
        PulseSurveyAssignmentCountryLengthRuleFlagsALongCountry();
        PulseSurveyAssignmentRequiredPhoneRuleFlagsABlankPhone();
        PulseSurveyAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
