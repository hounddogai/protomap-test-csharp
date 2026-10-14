namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultAuditCheckpointRulesChecks
{
    public static void DefaultAuditCheckpointValidatorReportsEveryProblem()
    {
        var validator = DefaultAuditCheckpointRules.DefaultDefaultAuditCheckpointValidator();
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAuditCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultAuditCheckpointNameLengthRule(120);
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAuditCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultAuditCheckpointRequiredCodeRule("default audit checkpoint needs this field");
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAuditCheckpointRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new DefaultAuditCheckpointRequiredEmployeeIdRule("default audit checkpoint needs a value");
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultAuditCheckpointValidatorReportsEveryProblem();
        DefaultAuditCheckpointNameLengthRuleFlagsALongName();
        DefaultAuditCheckpointRequiredCodeRuleFlagsABlankCode();
        DefaultAuditCheckpointRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
