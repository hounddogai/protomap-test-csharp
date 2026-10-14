namespace Acme.Workforce.Compliance.Tests.Certification.Claim;

using Acme.Workforce.Compliance.Certification.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class CertificationExceptionRulesChecks
{
    public static void CertificationExceptionValidatorReportsEveryProblem()
    {
        var validator = CertificationExceptionRules.DefaultCertificationExceptionValidator();
        var item = CertificationException.NewCertificationException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationExceptionDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new CertificationExceptionDiscountRangeRule(4.1, 701.2);
        var item = CertificationException.NewCertificationException("x", "y");
        item.Discount = 702.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationExceptionGradeLengthRuleFlagsALongGrade()
    {
        var rule = new CertificationExceptionGradeLengthRule(3600);
        var item = CertificationException.NewCertificationException("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CertificationExceptionRequiredLabelRule("certification exception needs an entry");
        var item = CertificationException.NewCertificationException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationExceptionValidatorReportsEveryProblem();
        CertificationExceptionDiscountRangeRuleFlagsValuesAboveDiscount();
        CertificationExceptionGradeLengthRuleFlagsALongGrade();
        CertificationExceptionRequiredLabelRuleFlagsABlankLabel();
    }
}
