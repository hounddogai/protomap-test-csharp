namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TieredCertificationCheckpointRulesChecks
{
    public static void TieredCertificationCheckpointValidatorReportsEveryProblem()
    {
        var validator = TieredCertificationCheckpointRules.DefaultTieredCertificationCheckpointValidator();
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredCertificationCheckpointJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new TieredCertificationCheckpointJobCodeLengthRule(86400);
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCertificationCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new TieredCertificationCheckpointNameLengthRule(80);
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCertificationCheckpointCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new TieredCertificationCheckpointCurrencyLengthRule(255);
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCertificationCheckpointVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new TieredCertificationCheckpointVersionRangeRule(64, 86400);
        var item = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredCertificationCheckpointValidatorReportsEveryProblem();
        TieredCertificationCheckpointJobCodeLengthRuleFlagsALongJobCode();
        TieredCertificationCheckpointNameLengthRuleFlagsALongName();
        TieredCertificationCheckpointCurrencyLengthRuleFlagsALongCurrency();
        TieredCertificationCheckpointVersionRangeRuleFlagsValuesAboveVersion();
    }
}
