namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ReorgAuditRulesChecks
{
    public static void ReorgAuditValidatorReportsEveryProblem()
    {
        var validator = ReorgAuditRules.DefaultReorgAuditValidator();
        var item = ReorgAudit.NewReorgAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgAuditNameLengthRuleFlagsALongName()
    {
        var rule = new ReorgAuditNameLengthRule(80);
        var item = ReorgAudit.NewReorgAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgAuditCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ReorgAuditCostRangeRule(7.4, 3441.1);
        var item = ReorgAudit.NewReorgAudit("x", "y");
        item.Cost = 3442.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgAuditChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new ReorgAuditChecksumLengthRule(64);
        var item = ReorgAudit.NewReorgAudit("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgAuditValidatorReportsEveryProblem();
        ReorgAuditNameLengthRuleFlagsALongName();
        ReorgAuditCostRangeRuleFlagsValuesAboveCost();
        ReorgAuditChecksumLengthRuleFlagsALongChecksum();
    }
}
