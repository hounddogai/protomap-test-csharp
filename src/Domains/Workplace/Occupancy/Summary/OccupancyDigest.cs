namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy digest is in its life cycle.</summary>
public enum OccupancyDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A occupancy digest. Its code is unique within a tenant.</summary>
public sealed class OccupancyDigest(string code, string label, double score, string country, string category, int createdAt, int endsAt, int durationMinutes, string managerId, OccupancyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public int CreatedAt { get; set; } = createdAt;
    public int EndsAt { get; set; } = endsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ManagerId { get; set; } = managerId;
    public OccupancyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy digest in status idle.</summary>
    public static OccupancyDigest NewOccupancyDigest(string code, string label)
    {
        return new OccupancyDigest(code, label, 0.0, "", "", 0, 0, 0, "", OccupancyDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyDigestStatus next)
    {
        switch (Status)
        {
            case OccupancyDigestStatus.Idle:
                switch (next)
                {
                    case OccupancyDigestStatus.Ready:
                        return true;
                }
                return false;
            case OccupancyDigestStatus.Warming:
                switch (next)
                {
                    case OccupancyDigestStatus.Ready:
                    case OccupancyDigestStatus.Draining:
                        return true;
                }
                return false;
            case OccupancyDigestStatus.Ready:
                return next == OccupancyDigestStatus.Draining;
            case OccupancyDigestStatus.Draining:
                switch (next)
                {
                    case OccupancyDigestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"occupancy digest"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Score - (double)DurationMinutes, 2);
    }

    public OccupancyDigest Copy()
    {
        return new OccupancyDigest(
            Code,
            Label,
            Score,
            Country,
            Category,
            CreatedAt,
            EndsAt,
            DurationMinutes,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
