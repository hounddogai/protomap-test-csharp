namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskPreferenceChecks
{
    public static void DeskPreferenceStartsInCreated()
    {
        var item = DeskPreference.NewDeskPreference("code-865", "label-796");
        Check.Equal(item.Status, DeskPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskPreferenceMovesToValidated()
    {
        var item = DeskPreference.NewDeskPreference("code-865", "label-796");
        try
        {
            item.MoveTo(DeskPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskPreferenceStatus.Validated, "status");
    }

    public static void DeskPreferenceRejectsMovesFromRetired()
    {
        var item = DeskPreference.NewDeskPreference("code-865", "label-796");
        item.Status = DeskPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(DeskPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeskPreferenceStartsInCreated();
        DeskPreferenceMovesToValidated();
        DeskPreferenceRejectsMovesFromRetired();
    }
}
