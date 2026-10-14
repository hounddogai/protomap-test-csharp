namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MobilityRuleChecks
{
    public static void MobilityRuleStartsInRequested()
    {
        var item = MobilityRule.NewMobilityRule("code-802", "name-35");
        Check.Equal(item.Status, MobilityRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityRuleMovesToGranted()
    {
        var item = MobilityRule.NewMobilityRule("code-802", "name-35");
        try
        {
            item.MoveTo(MobilityRuleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityRuleStatus.Granted, "status");
    }

    public static void MobilityRuleRejectsMovesFromExpired()
    {
        var item = MobilityRule.NewMobilityRule("code-802", "name-35");
        item.Status = MobilityRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(MobilityRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        MobilityRuleStartsInRequested();
        MobilityRuleMovesToGranted();
        MobilityRuleRejectsMovesFromExpired();
    }
}
