namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackPositionQuotaViewChecks
{
    public static void FallbackPositionQuotaViewShowsTheStatusInCapitals()
    {
        var item = FallbackPositionQuota.NewFallbackPositionQuota("code-980", "title-730");
        var view = FallbackPositionQuotaView.ToFallbackPositionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPositionQuotaView.ToFallbackPositionQuotaViews(new List<FallbackPositionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPositionQuotaViewShowsTheStatusInCapitals();
    }
}
