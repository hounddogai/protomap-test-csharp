namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional span review is in its life cycle.</summary>
public enum RegionalSpanReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A regional span review as the span team keeps it.</summary>
public sealed class RegionalSpanReview(string code, string label, double floorValue, string currency, int expiresAt, RegionalSpanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Currency { get; set; } = currency;
    public int ExpiresAt { get; set; } = expiresAt;
    public RegionalSpanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a regional span review in status requested.</summary>
    public static RegionalSpanReview NewRegionalSpanReview(string code, string label)
    {
        return new RegionalSpanReview(code, label, 0.0, "", 0, RegionalSpanReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSpanReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSpanReviewStatus next)
    {
        switch (Status)
        {
            case RegionalSpanReviewStatus.Requested:
                return next == RegionalSpanReviewStatus.Revoked || next == RegionalSpanReviewStatus.Expired;
            case RegionalSpanReviewStatus.Granted:
                switch (next)
                {
                    case RegionalSpanReviewStatus.Revoked:
                    case RegionalSpanReviewStatus.Expired:
                        return true;
                }
                return false;
            case RegionalSpanReviewStatus.Revoked:
                return next == RegionalSpanReviewStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalSpanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional span review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(FloorValue - (double)ExpiresAt, 2);
    }

    public RegionalSpanReview Copy()
    {
        return new RegionalSpanReview(Code, Label, FloorValue, Currency, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
