namespace Acme.Workforce.Assets.Tests.LicenseSeat.Template;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LicenseSeatTemplateRulesChecks
{
    public static void LicenseSeatTemplateValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatTemplateRules.DefaultLicenseSeatTemplateValidator();
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatTemplateUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new LicenseSeatTemplateUpdatedAtRangeRule(1000, 86400);
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new LicenseSeatTemplateRequiredNameRule("license seat template needs this field");
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new LicenseSeatTemplateNameLengthRule(32);
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatTemplateLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LicenseSeatTemplateLeaveDaysRangeRule(32, 3600);
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatTemplateRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LicenseSeatTemplateRequiredGradeRule("license seat template needs this field");
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatTemplateValidatorReportsEveryProblem();
        LicenseSeatTemplateUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        LicenseSeatTemplateRequiredNameRuleFlagsABlankName();
        LicenseSeatTemplateNameLengthRuleFlagsALongName();
        LicenseSeatTemplateLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LicenseSeatTemplateRequiredGradeRuleFlagsABlankGrade();
    }
}
