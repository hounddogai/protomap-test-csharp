namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyRequestChecks
{
    public static void WarrantyRequestStartsInProposed()
    {
        var item = WarrantyRequest.NewWarrantyRequest("code-939", "title-663");
        Check.Equal(item.Status, WarrantyRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyRequestMovesToWithdrawn()
    {
        var item = WarrantyRequest.NewWarrantyRequest("code-939", "title-663");
        try
        {
            item.MoveTo(WarrantyRequestStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyRequestStatus.Withdrawn, "status");
    }

    public static void WarrantyRequestRejectsMovesFromWithdrawn()
    {
        var item = WarrantyRequest.NewWarrantyRequest("code-939", "title-663");
        item.Status = WarrantyRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WarrantyRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WarrantyRequestStartsInProposed();
        WarrantyRequestMovesToWithdrawn();
        WarrantyRequestRejectsMovesFromWithdrawn();
    }
}
