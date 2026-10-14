namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedHeadcountTemplateViewChecks
{
    public static void BatchedHeadcountTemplateViewShowsTheStatusInCapitals()
    {
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("code-9", "name-529");
        var view = BatchedHeadcountTemplateView.ToBatchedHeadcountTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedHeadcountTemplateView.ToBatchedHeadcountTemplateViews(new List<BatchedHeadcountTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedHeadcountTemplateViewShowsTheStatusInCapitals();
    }
}
