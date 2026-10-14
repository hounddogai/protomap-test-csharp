namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRecordChecks
{
    public static void CappedLaptopRecordStartsInDraft()
    {
        var item = CappedLaptopRecord.NewCappedLaptopRecord("code-941", "name-67");
        Check.Equal(item.Status, CappedLaptopRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedLaptopRecordMovesToActive()
    {
        var item = CappedLaptopRecord.NewCappedLaptopRecord("code-941", "name-67");
        try
        {
            item.MoveTo(CappedLaptopRecordStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedLaptopRecordStatus.Active, "status");
    }

    public static void CappedLaptopRecordRejectsMovesFromClosed()
    {
        var item = CappedLaptopRecord.NewCappedLaptopRecord("code-941", "name-67");
        item.Status = CappedLaptopRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedLaptopRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedLaptopRecordStartsInDraft();
        CappedLaptopRecordMovesToActive();
        CappedLaptopRecordRejectsMovesFromClosed();
    }
}
