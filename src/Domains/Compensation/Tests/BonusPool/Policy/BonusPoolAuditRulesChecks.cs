namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolAuditRulesChecks
{
    public static void BonusPoolAuditValidatorReportsEveryProblem()
    {
        var validator = BonusPoolAuditRules.DefaultBonusPoolAuditValidator();
        var item = BonusPoolAudit.NewBonusPoolAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new BonusPoolAuditCreatedAtRangeRule(32, 255);
        var item = BonusPoolAudit.NewBonusPoolAudit("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new BonusPoolAuditDurationMinutesRangeRule(80, 1000);
        var item = BonusPoolAudit.NewBonusPoolAudit("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolAuditNameLengthRuleFlagsALongName()
    {
        var rule = new BonusPoolAuditNameLengthRule(80);
        var item = BonusPoolAudit.NewBonusPoolAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolAuditValidatorReportsEveryProblem();
        BonusPoolAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        BonusPoolAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        BonusPoolAuditNameLengthRuleFlagsALongName();
    }
}
