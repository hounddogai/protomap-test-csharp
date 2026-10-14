namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeBatchViewChecks
{
    public static void AccessBadgeBatchViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeBatch.NewAccessBadgeBatch("code-739", "name-687");
        var view = AccessBadgeBatchView.ToAccessBadgeBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeBatchView.ToAccessBadgeBatchViews(new List<AccessBadgeBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeBatchViewShowsTheStatusInCapitals();
    }
}
