namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate manifest is in its life cycle.</summary>
public enum RateManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rate manifest as the rate team keeps it.</summary>
public sealed class RateManifest(string code, string label, string checksum, int expiresAt, int durationMinutes, int leaveDays, int batchSize, string category, int endsAt, double amount, RateManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public int ExpiresAt { get; set; } = expiresAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public int EndsAt { get; set; } = endsAt;
    public double Amount { get; set; } = amount;
    public RateManifestStatus Status { get; set; } = status;

    /// <summary>Starts a rate manifest in status open.</summary>
    public static RateManifest NewRateManifest(string code, string label)
    {
        return new RateManifest(code, label, "", 0, 0, 0, 0, "", 0, 0.0, RateManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateManifestStatus next)
    {
        switch (Status)
        {
            case RateManifestStatus.Open:
                return next == RateManifestStatus.Reopened;
            case RateManifestStatus.InReview:
                switch (next)
                {
                    case RateManifestStatus.Archived:
                        return true;
                }
                return false;
            case RateManifestStatus.Resolved:
                return next == RateManifestStatus.Archived;
            case RateManifestStatus.Reopened:
                switch (next)
                {
                    case RateManifestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)BatchSize + (double)LeaveDays, 2);
    }

    public RateManifest Copy()
    {
        return new RateManifest(
            Code,
            Label,
            Checksum,
            ExpiresAt,
            DurationMinutes,
            LeaveDays,
            BatchSize,
            Category,
            EndsAt,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
