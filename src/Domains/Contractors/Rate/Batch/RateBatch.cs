namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate batch is in its life cycle.</summary>
public enum RateBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rate batch and its current status.</summary>
public sealed class RateBatch(string code, string title, int version, string segment, string badge, int revision, string currency, double floorValue, bool visible, int startsAt, RateBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Segment { get; set; } = segment;
    public string Badge { get; set; } = badge;
    public int Revision { get; set; } = revision;
    public string Currency { get; set; } = currency;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public int StartsAt { get; set; } = startsAt;
    public RateBatchStatus Status { get; set; } = status;

    /// <summary>Starts a rate batch in status open.</summary>
    public static RateBatch NewRateBatch(string code, string title)
    {
        return new RateBatch(code, title, 0, "", "", 0, "", 0.0, false, 0, RateBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateBatchStatus next)
    {
        switch (Status)
        {
            case RateBatchStatus.Open:
                return next == RateBatchStatus.Archived;
            case RateBatchStatus.InReview:
                return next == RateBatchStatus.Reopened;
            case RateBatchStatus.Resolved:
                switch (next)
                {
                    case RateBatchStatus.Reopened:
                    case RateBatchStatus.Archived:
                        return true;
                }
                return false;
            case RateBatchStatus.Reopened:
                switch (next)
                {
                    case RateBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"rate batch"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version + (double)StartsAt, 2);
    }

    public RateBatch Copy()
    {
        return new RateBatch(
            Code,
            Title,
            Version,
            Segment,
            Badge,
            Revision,
            Currency,
            FloorValue,
            Visible,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
