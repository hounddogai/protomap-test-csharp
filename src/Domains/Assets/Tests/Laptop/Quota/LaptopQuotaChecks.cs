namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopQuotaChecks
{
    public static void LaptopQuotaStartsInOpen()
    {
        var item = LaptopQuota.NewLaptopQuota("code-370", "label-726");
        Check.Equal(item.Status, LaptopQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopQuotaMovesToResolved()
    {
        var item = LaptopQuota.NewLaptopQuota("code-370", "label-726");
        try
        {
            item.MoveTo(LaptopQuotaStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopQuotaStatus.Resolved, "status");
    }

    public static void LaptopQuotaRejectsMovesFromArchived()
    {
        var item = LaptopQuota.NewLaptopQuota("code-370", "label-726");
        item.Status = LaptopQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(LaptopQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        LaptopQuotaStartsInOpen();
        LaptopQuotaMovesToResolved();
        LaptopQuotaRejectsMovesFromArchived();
    }
}
