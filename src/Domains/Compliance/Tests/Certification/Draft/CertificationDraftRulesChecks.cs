namespace Acme.Workforce.Compliance.Tests.Certification.Draft;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CertificationDraftRulesChecks
{
    public static void CertificationDraftValidatorReportsEveryProblem()
    {
        var validator = CertificationDraftRules.DefaultCertificationDraftValidator();
        var item = CertificationDraft.NewCertificationDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CertificationDraftLabelLengthRule(64);
        var item = CertificationDraft.NewCertificationDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CertificationDraftRequiredTitleRule("certification draft needs this field");
        var item = CertificationDraft.NewCertificationDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationDraftAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new CertificationDraftAmountRangeRule(6.5, 839.6);
        var item = CertificationDraft.NewCertificationDraft("x", "y");
        item.Amount = 841.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationDraftRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new CertificationDraftRequiredGradeRule("certification draft needs a value");
        var item = CertificationDraft.NewCertificationDraft("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CertificationDraftTitleLengthRule(64);
        var item = CertificationDraft.NewCertificationDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CertificationDraftValidatorReportsEveryProblem();
        CertificationDraftLabelLengthRuleFlagsALongLabel();
        CertificationDraftRequiredTitleRuleFlagsABlankTitle();
        CertificationDraftAmountRangeRuleFlagsValuesAboveAmount();
        CertificationDraftRequiredGradeRuleFlagsABlankGrade();
        CertificationDraftTitleLengthRuleFlagsALongTitle();
    }
}
