namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorPreferenceChecks
{
    public static void VisitorPreferenceStartsInRequested()
    {
        var item = VisitorPreference.NewVisitorPreference("code-491", "title-634");
        Check.Equal(item.Status, VisitorPreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorPreferenceMovesToRevoked()
    {
        var item = VisitorPreference.NewVisitorPreference("code-491", "title-634");
        try
        {
            item.MoveTo(VisitorPreferenceStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorPreferenceStatus.Revoked, "status");
    }

    public static void VisitorPreferenceRejectsMovesFromExpired()
    {
        var item = VisitorPreference.NewVisitorPreference("code-491", "title-634");
        item.Status = VisitorPreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorPreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorPreferenceStartsInRequested();
        VisitorPreferenceMovesToRevoked();
        VisitorPreferenceRejectsMovesFromExpired();
    }
}
