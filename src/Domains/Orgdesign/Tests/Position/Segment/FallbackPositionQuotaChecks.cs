namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackPositionQuotaChecks
{
    public static void FallbackPositionQuotaStartsInOpen()
    {
        var item = FallbackPositionQuota.NewFallbackPositionQuota("code-980", "title-730");
        Check.Equal(item.Status, FallbackPositionQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPositionQuotaMovesToInReview()
    {
        var item = FallbackPositionQuota.NewFallbackPositionQuota("code-980", "title-730");
        try
        {
            item.MoveTo(FallbackPositionQuotaStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPositionQuotaStatus.InReview, "status");
    }

    public static void FallbackPositionQuotaRejectsMovesFromArchived()
    {
        var item = FallbackPositionQuota.NewFallbackPositionQuota("code-980", "title-730");
        item.Status = FallbackPositionQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackPositionQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackPositionQuotaStartsInOpen();
        FallbackPositionQuotaMovesToInReview();
        FallbackPositionQuotaRejectsMovesFromArchived();
    }
}
