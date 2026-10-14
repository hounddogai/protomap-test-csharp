namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanQuotaChecks
{
    public static void SpanQuotaStartsInRequested()
    {
        var item = SpanQuota.NewSpanQuota("code-632", "label-861");
        Check.Equal(item.Status, SpanQuotaStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanQuotaMovesToRevoked()
    {
        var item = SpanQuota.NewSpanQuota("code-632", "label-861");
        try
        {
            item.MoveTo(SpanQuotaStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanQuotaStatus.Revoked, "status");
    }

    public static void SpanQuotaRejectsMovesFromExpired()
    {
        var item = SpanQuota.NewSpanQuota("code-632", "label-861");
        item.Status = SpanQuotaStatus.Expired;
        Check.Fails(() => item.MoveTo(SpanQuotaStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SpanQuotaStartsInRequested();
        SpanQuotaMovesToRevoked();
        SpanQuotaRejectsMovesFromExpired();
    }
}
