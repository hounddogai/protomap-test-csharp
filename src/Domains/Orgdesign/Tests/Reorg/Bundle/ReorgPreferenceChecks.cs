namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgPreferenceChecks
{
    public static void ReorgPreferenceStartsInOpen()
    {
        var item = ReorgPreference.NewReorgPreference("code-783", "title-96");
        Check.Equal(item.Status, ReorgPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgPreferenceMovesToInReview()
    {
        var item = ReorgPreference.NewReorgPreference("code-783", "title-96");
        try
        {
            item.MoveTo(ReorgPreferenceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgPreferenceStatus.InReview, "status");
    }

    public static void ReorgPreferenceRejectsMovesFromArchived()
    {
        var item = ReorgPreference.NewReorgPreference("code-783", "title-96");
        item.Status = ReorgPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(ReorgPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReorgPreferenceStartsInOpen();
        ReorgPreferenceMovesToInReview();
        ReorgPreferenceRejectsMovesFromArchived();
    }
}
