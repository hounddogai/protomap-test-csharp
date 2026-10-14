namespace Acme.Workforce.Assets.Tests.Assignment.Profile;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedAssignmentPreferenceChecks
{
    public static void CappedAssignmentPreferenceStartsInCreated()
    {
        var item = CappedAssignmentPreference.NewCappedAssignmentPreference("code-540", "label-590");
        Check.Equal(item.Status, CappedAssignmentPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAssignmentPreferenceMovesToPublished()
    {
        var item = CappedAssignmentPreference.NewCappedAssignmentPreference("code-540", "label-590");
        try
        {
            item.MoveTo(CappedAssignmentPreferenceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAssignmentPreferenceStatus.Published, "status");
    }

    public static void CappedAssignmentPreferenceRejectsMovesFromRetired()
    {
        var item = CappedAssignmentPreference.NewCappedAssignmentPreference("code-540", "label-590");
        item.Status = CappedAssignmentPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedAssignmentPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedAssignmentPreferenceStartsInCreated();
        CappedAssignmentPreferenceMovesToPublished();
        CappedAssignmentPreferenceRejectsMovesFromRetired();
    }
}
