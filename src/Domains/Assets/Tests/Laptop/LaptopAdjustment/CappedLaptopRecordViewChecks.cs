namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRecordViewChecks
{
    public static void CappedLaptopRecordViewShowsTheStatusInCapitals()
    {
        var item = CappedLaptopRecord.NewCappedLaptopRecord("code-941", "name-67");
        var view = CappedLaptopRecordView.ToCappedLaptopRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedLaptopRecordView.ToCappedLaptopRecordViews(new List<CappedLaptopRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedLaptopRecordViewShowsTheStatusInCapitals();
    }
}
