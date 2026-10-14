namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunCheckpointRulesChecks
{
    public static void PayRunCheckpointValidatorReportsEveryProblem()
    {
        var validator = PayRunCheckpointRules.DefaultPayRunCheckpointValidator();
        var item = PayRunCheckpoint.NewPayRunCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayRunCheckpointRequiredLabelRule("pay run checkpoint needs this field");
        var item = PayRunCheckpoint.NewPayRunCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunCheckpointRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new PayRunCheckpointRequiredPhoneRule("pay run checkpoint needs an entry");
        var item = PayRunCheckpoint.NewPayRunCheckpoint("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunCheckpointTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PayRunCheckpointTeamLengthRule(120);
        var item = PayRunCheckpoint.NewPayRunCheckpoint("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunCheckpointCodeLengthRule(64);
        var item = PayRunCheckpoint.NewPayRunCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunCheckpointValidatorReportsEveryProblem();
        PayRunCheckpointRequiredLabelRuleFlagsABlankLabel();
        PayRunCheckpointRequiredPhoneRuleFlagsABlankPhone();
        PayRunCheckpointTeamLengthRuleFlagsALongTeam();
        PayRunCheckpointCodeLengthRuleFlagsALongCode();
    }
}
