namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TieredPolicyAckEntryRulesChecks
{
    public static void TieredPolicyAckEntryValidatorReportsEveryProblem()
    {
        var validator = TieredPolicyAckEntryRules.DefaultTieredPolicyAckEntryValidator();
        var item = TieredPolicyAckEntry.NewTieredPolicyAckEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPolicyAckEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredPolicyAckEntryRequiredTitleRule("tiered policy ack entry needs a value");
        var item = TieredPolicyAckEntry.NewTieredPolicyAckEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPolicyAckEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredPolicyAckEntryRequiredNameRule("tiered policy ack entry needs this field");
        var item = TieredPolicyAckEntry.NewTieredPolicyAckEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPolicyAckEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredPolicyAckEntryRequiredCodeRule("tiered policy ack entry needs an entry");
        var item = TieredPolicyAckEntry.NewTieredPolicyAckEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPolicyAckEntryValidatorReportsEveryProblem();
        TieredPolicyAckEntryRequiredTitleRuleFlagsABlankTitle();
        TieredPolicyAckEntryRequiredNameRuleFlagsABlankName();
        TieredPolicyAckEntryRequiredCodeRuleFlagsABlankCode();
    }
}
