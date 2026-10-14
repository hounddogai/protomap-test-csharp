namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalOffboardingNoticeChecks
{
    public static void RegionalOffboardingNoticeStartsInDraft()
    {
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("code-392", "label-491");
        Check.Equal(item.Status, RegionalOffboardingNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalOffboardingNoticeMovesToPending()
    {
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("code-392", "label-491");
        try
        {
            item.MoveTo(RegionalOffboardingNoticeStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalOffboardingNoticeStatus.Pending, "status");
    }

    public static void RegionalOffboardingNoticeRejectsMovesFromClosed()
    {
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("code-392", "label-491");
        item.Status = RegionalOffboardingNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalOffboardingNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalOffboardingNoticeStartsInDraft();
        RegionalOffboardingNoticeMovesToPending();
        RegionalOffboardingNoticeRejectsMovesFromClosed();
    }
}
