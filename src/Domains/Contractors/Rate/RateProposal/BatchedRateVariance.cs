namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched rate variance is in its life cycle.</summary>
public enum BatchedRateVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched rate variance. Its code is unique within a tenant.</summary>
public sealed class BatchedRateVariance(string code, string title, string locale, int expiresAt, int capacity, int startsAt, int attempts, int version, double cost, BatchedRateVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public int Attempts { get; set; } = attempts;
    public int Version { get; set; } = version;
    public double Cost { get; set; } = cost;
    public BatchedRateVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a batched rate variance in status requested.</summary>
    public static BatchedRateVariance NewBatchedRateVariance(string code, string title)
    {
        return new BatchedRateVariance(code, title, "", 0, 0, 0, 0, 0, 0.0, BatchedRateVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRateVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRateVarianceStatus next)
    {
        switch (Status)
        {
            case BatchedRateVarianceStatus.Requested:
                switch (next)
                {
                    case BatchedRateVarianceStatus.Revoked:
                        return true;
                }
                return false;
            case BatchedRateVarianceStatus.Granted:
                switch (next)
                {
                    case BatchedRateVarianceStatus.Expired:
                        return true;
                }
                return false;
            case BatchedRateVarianceStatus.Revoked:
                return next == BatchedRateVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedRateVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched rate variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"batched rate variance"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Capacity + (double)ExpiresAt, 2);
    }

    public BatchedRateVariance Copy()
    {
        return new BatchedRateVariance(
            Code,
            Title,
            Locale,
            ExpiresAt,
            Capacity,
            StartsAt,
            Attempts,
            Version,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
