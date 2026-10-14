namespace Acme.Workforce.Workplace.Tests.Visitor.Checkpoint;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientVisitorPreferenceChecks
{
    public static void LenientVisitorPreferenceStartsInDraft()
    {
        var item = LenientVisitorPreference.NewLenientVisitorPreference("code-739", "label-558");
        Check.Equal(item.Status, LenientVisitorPreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVisitorPreferenceMovesToActive()
    {
        var item = LenientVisitorPreference.NewLenientVisitorPreference("code-739", "label-558");
        try
        {
            item.MoveTo(LenientVisitorPreferenceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVisitorPreferenceStatus.Active, "status");
    }

    public static void LenientVisitorPreferenceRejectsMovesFromClosed()
    {
        var item = LenientVisitorPreference.NewLenientVisitorPreference("code-739", "label-558");
        item.Status = LenientVisitorPreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientVisitorPreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientVisitorPreferenceStartsInDraft();
        LenientVisitorPreferenceMovesToActive();
        LenientVisitorPreferenceRejectsMovesFromClosed();
    }
}
