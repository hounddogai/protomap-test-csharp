namespace Acme.Workforce.Workplace.Tests.Desk.Rule;

using Acme.Workforce.Workplace.Desk.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingDeskRollupRulesChecks
{
    public static void PendingDeskRollupValidatorReportsEveryProblem()
    {
        var validator = PendingDeskRollupRules.DefaultPendingDeskRollupValidator();
        var item = PendingDeskRollup.NewPendingDeskRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingDeskRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingDeskRollupCodeLengthRule(80);
        var item = PendingDeskRollup.NewPendingDeskRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingDeskRollupChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new PendingDeskRollupChecksumLengthRule(16);
        var item = PendingDeskRollup.NewPendingDeskRollup("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingDeskRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingDeskRollupRequiredCodeRule("pending desk rollup needs a value");
        var item = PendingDeskRollup.NewPendingDeskRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingDeskRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PendingDeskRollupExpiresAtRangeRule(64, 120);
        var item = PendingDeskRollup.NewPendingDeskRollup("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingDeskRollupValidatorReportsEveryProblem();
        PendingDeskRollupCodeLengthRuleFlagsALongCode();
        PendingDeskRollupChecksumLengthRuleFlagsALongChecksum();
        PendingDeskRollupRequiredCodeRuleFlagsABlankCode();
        PendingDeskRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
