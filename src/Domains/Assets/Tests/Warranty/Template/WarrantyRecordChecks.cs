namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyRecordChecks
{
    public static void WarrantyRecordStartsInIdle()
    {
        var item = WarrantyRecord.NewWarrantyRecord("code-585", "title-639");
        Check.Equal(item.Status, WarrantyRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyRecordMovesToWarming()
    {
        var item = WarrantyRecord.NewWarrantyRecord("code-585", "title-639");
        try
        {
            item.MoveTo(WarrantyRecordStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyRecordStatus.Warming, "status");
    }

    public static void WarrantyRecordRejectsMovesFromStopped()
    {
        var item = WarrantyRecord.NewWarrantyRecord("code-585", "title-639");
        item.Status = WarrantyRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantyRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantyRecordStartsInIdle();
        WarrantyRecordMovesToWarming();
        WarrantyRecordRejectsMovesFromStopped();
    }
}
