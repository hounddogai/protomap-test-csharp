namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredLaptopNoticeViewChecks
{
    public static void TieredLaptopNoticeViewShowsTheStatusInCapitals()
    {
        var item = TieredLaptopNotice.NewTieredLaptopNotice("code-430", "title-900");
        var view = TieredLaptopNoticeView.ToTieredLaptopNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredLaptopNoticeView.ToTieredLaptopNoticeViews(new List<TieredLaptopNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredLaptopNoticeViewShowsTheStatusInCapitals();
    }
}
