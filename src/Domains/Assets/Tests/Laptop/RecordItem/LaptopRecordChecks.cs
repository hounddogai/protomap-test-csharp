namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopRecordChecks
{
    public static void LaptopRecordStartsInProposed()
    {
        var item = LaptopRecord.NewLaptopRecord("code-726", "label-642");
        Check.Equal(item.Status, LaptopRecordStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopRecordMovesToApproved()
    {
        var item = LaptopRecord.NewLaptopRecord("code-726", "label-642");
        try
        {
            item.MoveTo(LaptopRecordStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopRecordStatus.Approved, "status");
    }

    public static void LaptopRecordRejectsMovesFromWithdrawn()
    {
        var item = LaptopRecord.NewLaptopRecord("code-726", "label-642");
        item.Status = LaptopRecordStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LaptopRecordStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LaptopRecordStartsInProposed();
        LaptopRecordMovesToApproved();
        LaptopRecordRejectsMovesFromWithdrawn();
    }
}
