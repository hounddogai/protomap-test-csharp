namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyMappingChecks
{
    public static void WarrantyMappingStartsInPlanned()
    {
        var item = WarrantyMapping.NewWarrantyMapping("code-155", "label-791");
        Check.Equal(item.Status, WarrantyMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyMappingMovesToInProgress()
    {
        var item = WarrantyMapping.NewWarrantyMapping("code-155", "label-791");
        try
        {
            item.MoveTo(WarrantyMappingStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyMappingStatus.InProgress, "status");
    }

    public static void WarrantyMappingRejectsMovesFromCancelled()
    {
        var item = WarrantyMapping.NewWarrantyMapping("code-155", "label-791");
        item.Status = WarrantyMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WarrantyMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WarrantyMappingStartsInPlanned();
        WarrantyMappingMovesToInProgress();
        WarrantyMappingRejectsMovesFromCancelled();
    }
}
