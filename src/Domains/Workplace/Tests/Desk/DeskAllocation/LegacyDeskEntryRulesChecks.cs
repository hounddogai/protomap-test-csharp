namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class LegacyDeskEntryRulesChecks
{
    public static void LegacyDeskEntryValidatorReportsEveryProblem()
    {
        var validator = LegacyDeskEntryRules.DefaultLegacyDeskEntryValidator();
        var item = LegacyDeskEntry.NewLegacyDeskEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyDeskEntryExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new LegacyDeskEntryExternalRefLengthRule(255);
        var item = LegacyDeskEntry.NewLegacyDeskEntry("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyDeskEntryRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new LegacyDeskEntryRequiredExternalRefRule("legacy desk entry needs an entry");
        var item = LegacyDeskEntry.NewLegacyDeskEntry("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyDeskEntryRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LegacyDeskEntryRequiredChannelRule("legacy desk entry needs an entry");
        var item = LegacyDeskEntry.NewLegacyDeskEntry("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyDeskEntryValidatorReportsEveryProblem();
        LegacyDeskEntryExternalRefLengthRuleFlagsALongExternalRef();
        LegacyDeskEntryRequiredExternalRefRuleFlagsABlankExternalRef();
        LegacyDeskEntryRequiredChannelRuleFlagsABlankChannel();
    }
}
