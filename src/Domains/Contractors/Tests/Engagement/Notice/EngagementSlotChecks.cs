namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementSlotChecks
{
    public static void EngagementSlotStartsInOpen()
    {
        var item = EngagementSlot.NewEngagementSlot("code-225", "label-764");
        Check.Equal(item.Status, EngagementSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementSlotMovesToResolved()
    {
        var item = EngagementSlot.NewEngagementSlot("code-225", "label-764");
        try
        {
            item.MoveTo(EngagementSlotStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementSlotStatus.Resolved, "status");
    }

    public static void EngagementSlotRejectsMovesFromArchived()
    {
        var item = EngagementSlot.NewEngagementSlot("code-225", "label-764");
        item.Status = EngagementSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(EngagementSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        EngagementSlotStartsInOpen();
        EngagementSlotMovesToResolved();
        EngagementSlotRejectsMovesFromArchived();
    }
}
