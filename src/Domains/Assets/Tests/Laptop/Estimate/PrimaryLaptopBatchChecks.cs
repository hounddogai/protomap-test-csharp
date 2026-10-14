namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryLaptopBatchChecks
{
    public static void PrimaryLaptopBatchStartsInRequested()
    {
        var item = PrimaryLaptopBatch.NewPrimaryLaptopBatch("code-533", "label-700");
        Check.Equal(item.Status, PrimaryLaptopBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryLaptopBatchMovesToRevoked()
    {
        var item = PrimaryLaptopBatch.NewPrimaryLaptopBatch("code-533", "label-700");
        try
        {
            item.MoveTo(PrimaryLaptopBatchStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryLaptopBatchStatus.Revoked, "status");
    }

    public static void PrimaryLaptopBatchRejectsMovesFromExpired()
    {
        var item = PrimaryLaptopBatch.NewPrimaryLaptopBatch("code-533", "label-700");
        item.Status = PrimaryLaptopBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryLaptopBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryLaptopBatchStartsInRequested();
        PrimaryLaptopBatchMovesToRevoked();
        PrimaryLaptopBatchRejectsMovesFromExpired();
    }
}
