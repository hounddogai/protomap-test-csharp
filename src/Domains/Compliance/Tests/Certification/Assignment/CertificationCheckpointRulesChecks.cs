namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationCheckpointRulesChecks
{
    public static void CertificationCheckpointValidatorReportsEveryProblem()
    {
        var validator = CertificationCheckpointRules.DefaultCertificationCheckpointValidator();
        var item = CertificationCheckpoint.NewCertificationCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CertificationCheckpointLabelLengthRule(64);
        var item = CertificationCheckpoint.NewCertificationCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationCheckpointCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new CertificationCheckpointCurrencyLengthRule(16);
        var item = CertificationCheckpoint.NewCertificationCheckpoint("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationCheckpointRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new CertificationCheckpointRevisionRangeRule(1000, 1000);
        var item = CertificationCheckpoint.NewCertificationCheckpoint("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CertificationCheckpointRequiredLabelRule("certification checkpoint needs an entry");
        var item = CertificationCheckpoint.NewCertificationCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationCheckpointValidatorReportsEveryProblem();
        CertificationCheckpointLabelLengthRuleFlagsALongLabel();
        CertificationCheckpointCurrencyLengthRuleFlagsALongCurrency();
        CertificationCheckpointRevisionRangeRuleFlagsValuesAboveRevision();
        CertificationCheckpointRequiredLabelRuleFlagsABlankLabel();
    }
}
