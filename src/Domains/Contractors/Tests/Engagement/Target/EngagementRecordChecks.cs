namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementRecordChecks
{
    public static void EngagementRecordStartsInCreated()
    {
        var item = EngagementRecord.NewEngagementRecord("code-629", "title-18");
        Check.Equal(item.Status, EngagementRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementRecordMovesToPublished()
    {
        var item = EngagementRecord.NewEngagementRecord("code-629", "title-18");
        try
        {
            item.MoveTo(EngagementRecordStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementRecordStatus.Published, "status");
    }

    public static void EngagementRecordRejectsMovesFromRetired()
    {
        var item = EngagementRecord.NewEngagementRecord("code-629", "title-18");
        item.Status = EngagementRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(EngagementRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        EngagementRecordStartsInCreated();
        EngagementRecordMovesToPublished();
        EngagementRecordRejectsMovesFromRetired();
    }
}
