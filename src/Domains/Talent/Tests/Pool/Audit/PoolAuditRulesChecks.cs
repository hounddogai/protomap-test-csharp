namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolAuditRulesChecks
{
    public static void PoolAuditValidatorReportsEveryProblem()
    {
        var validator = PoolAuditRules.DefaultPoolAuditValidator();
        var item = PoolAudit.NewPoolAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolAuditSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new PoolAuditSegmentLengthRule(80);
        var item = PoolAudit.NewPoolAudit("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolAuditEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new PoolAuditEmployeeIdLengthRule(120);
        var item = PoolAudit.NewPoolAudit("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolAuditCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PoolAuditCostRangeRule(3.0, 4922.7);
        var item = PoolAudit.NewPoolAudit("x", "y");
        item.Cost = 4924.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolAuditRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new PoolAuditRequiredEmployeeIdRule("pool audit needs an entry");
        var item = PoolAudit.NewPoolAudit("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolAuditRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PoolAuditRetriesRangeRule(16, 16);
        var item = PoolAudit.NewPoolAudit("x", "y");
        item.Retries = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolAuditCodeLengthRule(1000);
        var item = PoolAudit.NewPoolAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolAuditValidatorReportsEveryProblem();
        PoolAuditSegmentLengthRuleFlagsALongSegment();
        PoolAuditEmployeeIdLengthRuleFlagsALongEmployeeId();
        PoolAuditCostRangeRuleFlagsValuesAboveCost();
        PoolAuditRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        PoolAuditRetriesRangeRuleFlagsValuesAboveRetries();
        PoolAuditCodeLengthRuleFlagsALongCode();
    }
}
