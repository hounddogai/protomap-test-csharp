namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy occupancy digest is in its life cycle.</summary>
public enum LegacyOccupancyDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy occupancy digest. Its code is unique within a tenant.</summary>
public sealed class LegacyOccupancyDigest(string code, string name, int durationMinutes, double margin, double rate, double rating, int updatedAt, string externalRef, double floorValue, string currency, LegacyOccupancyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Margin { get; set; } = margin;
    public double Rate { get; set; } = rate;
    public double Rating { get; set; } = rating;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ExternalRef { get; set; } = externalRef;
    public double FloorValue { get; set; } = floorValue;
    public string Currency { get; set; } = currency;
    public LegacyOccupancyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy occupancy digest in status planned.</summary>
    public static LegacyOccupancyDigest NewLegacyOccupancyDigest(string code, string name)
    {
        return new LegacyOccupancyDigest(
            code,
            name,
            0,
            0.0,
            0.0,
            0.0,
            0,
            "",
            0.0,
            "",
            LegacyOccupancyDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOccupancyDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOccupancyDigestStatus next)
    {
        switch (Status)
        {
            case LegacyOccupancyDigestStatus.Planned:
                return next == LegacyOccupancyDigestStatus.InProgress;
            case LegacyOccupancyDigestStatus.Scheduled:
                return next == LegacyOccupancyDigestStatus.Completed || next == LegacyOccupancyDigestStatus.Cancelled;
            case LegacyOccupancyDigestStatus.InProgress:
                return next == LegacyOccupancyDigestStatus.Completed || next == LegacyOccupancyDigestStatus.Cancelled;
            case LegacyOccupancyDigestStatus.Completed:
                switch (next)
                {
                    case LegacyOccupancyDigestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyOccupancyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy occupancy digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy occupancy digest"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Margin * FloorValue, 2);
    }

    public LegacyOccupancyDigest Copy()
    {
        return new LegacyOccupancyDigest(
            Code,
            Name,
            DurationMinutes,
            Margin,
            Rate,
            Rating,
            UpdatedAt,
            ExternalRef,
            FloorValue,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
