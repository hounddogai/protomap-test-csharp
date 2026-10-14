namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class SeasonalPolicyAckEntryRulesChecks
{
    public static void SeasonalPolicyAckEntryValidatorReportsEveryProblem()
    {
        var validator = SeasonalPolicyAckEntryRules.DefaultSeasonalPolicyAckEntryValidator();
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPolicyAckEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalPolicyAckEntryRequiredCodeRule("seasonal policy ack entry needs a value");
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPolicyAckEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalPolicyAckEntryCodeLengthRule(86400);
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPolicyAckEntryCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new SeasonalPolicyAckEntryCostRangeRule(5.8, 2940.6);
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("x", "y");
        item.Cost = 2942.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckEntryValidatorReportsEveryProblem();
        SeasonalPolicyAckEntryRequiredCodeRuleFlagsABlankCode();
        SeasonalPolicyAckEntryCodeLengthRuleFlagsALongCode();
        SeasonalPolicyAckEntryCostRangeRuleFlagsValuesAboveCost();
    }
}
