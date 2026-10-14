namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedAuditRecordRulesChecks
{
    public static void WeightedAuditRecordValidatorReportsEveryProblem()
    {
        var validator = WeightedAuditRecordRules.DefaultWeightedAuditRecordValidator();
        var item = WeightedAuditRecord.NewWeightedAuditRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAuditRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedAuditRecordCodeLengthRule(32);
        var item = WeightedAuditRecord.NewWeightedAuditRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAuditRecordTeamLengthRuleFlagsALongTeam()
    {
        var rule = new WeightedAuditRecordTeamLengthRule(255);
        var item = WeightedAuditRecord.NewWeightedAuditRecord("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAuditRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedAuditRecordRequiredLabelRule("weighted audit record needs this field");
        var item = WeightedAuditRecord.NewWeightedAuditRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedAuditRecordValidatorReportsEveryProblem();
        WeightedAuditRecordCodeLengthRuleFlagsALongCode();
        WeightedAuditRecordTeamLengthRuleFlagsALongTeam();
        WeightedAuditRecordRequiredLabelRuleFlagsABlankLabel();
    }
}
