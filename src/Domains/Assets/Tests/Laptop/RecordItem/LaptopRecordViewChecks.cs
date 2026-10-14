namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopRecordViewChecks
{
    public static void LaptopRecordViewShowsTheStatusInCapitals()
    {
        var item = LaptopRecord.NewLaptopRecord("code-726", "label-642");
        var view = LaptopRecordView.ToLaptopRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopRecordView.ToLaptopRecordViews(new List<LaptopRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopRecordViewShowsTheStatusInCapitals();
    }
}
