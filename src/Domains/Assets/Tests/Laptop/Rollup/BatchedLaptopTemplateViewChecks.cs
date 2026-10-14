namespace Acme.Workforce.Assets.Tests.Laptop.Rollup;

using Acme.Workforce.Assets.Laptop.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedLaptopTemplateViewChecks
{
    public static void BatchedLaptopTemplateViewShowsTheStatusInCapitals()
    {
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("code-204", "title-639");
        var view = BatchedLaptopTemplateView.ToBatchedLaptopTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedLaptopTemplateView.ToBatchedLaptopTemplateViews(new List<BatchedLaptopTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedLaptopTemplateViewShowsTheStatusInCapitals();
    }
}
