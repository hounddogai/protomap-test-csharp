namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopQuotaViewChecks
{
    public static void LaptopQuotaViewShowsTheStatusInCapitals()
    {
        var item = LaptopQuota.NewLaptopQuota("code-370", "label-726");
        var view = LaptopQuotaView.ToLaptopQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopQuotaView.ToLaptopQuotaViews(new List<LaptopQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopQuotaViewShowsTheStatusInCapitals();
    }
}
