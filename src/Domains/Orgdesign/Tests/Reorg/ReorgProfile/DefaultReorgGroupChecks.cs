namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class DefaultReorgGroupChecks
{
    public static void DefaultReorgGroupStartsInCreated()
    {
        var item = DefaultReorgGroup.NewDefaultReorgGroup("code-997", "label-95");
        Check.Equal(item.Status, DefaultReorgGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultReorgGroupMovesToRetired()
    {
        var item = DefaultReorgGroup.NewDefaultReorgGroup("code-997", "label-95");
        try
        {
            item.MoveTo(DefaultReorgGroupStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultReorgGroupStatus.Retired, "status");
    }

    public static void DefaultReorgGroupRejectsMovesFromRetired()
    {
        var item = DefaultReorgGroup.NewDefaultReorgGroup("code-997", "label-95");
        item.Status = DefaultReorgGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultReorgGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultReorgGroupStartsInCreated();
        DefaultReorgGroupMovesToRetired();
        DefaultReorgGroupRejectsMovesFromRetired();
    }
}
