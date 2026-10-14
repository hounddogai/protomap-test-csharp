namespace Acme.Workforce.Workplace.Tests.Desk.DeskVariance;

using Acme.Workforce.Workplace.Desk.DeskVariance;

/// <summary>Checks of desk variance code, run by the project's runner.</summary>
public static class TieredDeskVarianceChecks
{
    public static void TieredDeskVarianceStartsInCreated()
    {
        var item = TieredDeskVariance.NewTieredDeskVariance("code-80", "label-65");
        Check.Equal(item.Status, TieredDeskVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredDeskVarianceMovesToValidated()
    {
        var item = TieredDeskVariance.NewTieredDeskVariance("code-80", "label-65");
        try
        {
            item.MoveTo(TieredDeskVarianceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredDeskVarianceStatus.Validated, "status");
    }

    public static void TieredDeskVarianceRejectsMovesFromRetired()
    {
        var item = TieredDeskVariance.NewTieredDeskVariance("code-80", "label-65");
        item.Status = TieredDeskVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredDeskVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredDeskVarianceStartsInCreated();
        TieredDeskVarianceMovesToValidated();
        TieredDeskVarianceRejectsMovesFromRetired();
    }
}
