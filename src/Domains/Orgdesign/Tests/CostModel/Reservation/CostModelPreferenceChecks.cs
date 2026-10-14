namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CostModelPreferenceChecks
{
    public static void CostModelPreferenceStartsInNew()
    {
        var item = CostModelPreference.NewCostModelPreference("code-321", "title-932");
        Check.Equal(item.Status, CostModelPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelPreferenceMovesToFailed()
    {
        var item = CostModelPreference.NewCostModelPreference("code-321", "title-932");
        try
        {
            item.MoveTo(CostModelPreferenceStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelPreferenceStatus.Failed, "status");
    }

    public static void CostModelPreferenceRejectsMovesFromFailed()
    {
        var item = CostModelPreference.NewCostModelPreference("code-321", "title-932");
        item.Status = CostModelPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(CostModelPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        CostModelPreferenceStartsInNew();
        CostModelPreferenceMovesToFailed();
        CostModelPreferenceRejectsMovesFromFailed();
    }
}
