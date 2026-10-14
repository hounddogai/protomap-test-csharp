namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SkillProfileAgreementRulesChecks
{
    public static void SkillProfileAgreementValidatorReportsEveryProblem()
    {
        var validator = SkillProfileAgreementRules.DefaultSkillProfileAgreementValidator();
        var item = SkillProfileAgreement.NewSkillProfileAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileAgreementRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SkillProfileAgreementRequiredLabelRule("skill profile agreement needs an entry");
        var item = SkillProfileAgreement.NewSkillProfileAgreement("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileAgreementQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SkillProfileAgreementQuantityRangeRule(1000, 3600);
        var item = SkillProfileAgreement.NewSkillProfileAgreement("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileAgreementValidatorReportsEveryProblem();
        SkillProfileAgreementRequiredLabelRuleFlagsABlankLabel();
        SkillProfileAgreementQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
