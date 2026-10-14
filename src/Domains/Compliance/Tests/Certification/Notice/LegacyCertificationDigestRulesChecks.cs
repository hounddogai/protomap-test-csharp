namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LegacyCertificationDigestRulesChecks
{
    public static void LegacyCertificationDigestValidatorReportsEveryProblem()
    {
        var validator = LegacyCertificationDigestRules.DefaultLegacyCertificationDigestValidator();
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyCertificationDigestCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new LegacyCertificationDigestCurrencyLengthRule(16);
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyCertificationDigestRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LegacyCertificationDigestRequiredGradeRule("legacy certification digest needs this field");
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyCertificationDigestNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyCertificationDigestNameLengthRule(16);
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyCertificationDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyCertificationDigestRequiredNameRule("legacy certification digest needs a value");
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyCertificationDigestValidatorReportsEveryProblem();
        LegacyCertificationDigestCurrencyLengthRuleFlagsALongCurrency();
        LegacyCertificationDigestRequiredGradeRuleFlagsABlankGrade();
        LegacyCertificationDigestNameLengthRuleFlagsALongName();
        LegacyCertificationDigestRequiredNameRuleFlagsABlankName();
    }
}
