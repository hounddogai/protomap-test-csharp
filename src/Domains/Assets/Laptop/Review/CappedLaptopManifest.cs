namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped laptop manifest is in its life cycle.</summary>
public enum CappedLaptopManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One capped laptop manifest, with what the planners need to act on it.</summary>
public sealed class CappedLaptopManifest(string code, string name, int sequence, bool approved, int updatedAt, int durationMinutes, string reviewer, string ownerId, CappedLaptopManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public bool Approved { get; set; } = approved;
    public int UpdatedAt { get; set; } = updatedAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Reviewer { get; set; } = reviewer;
    public string OwnerId { get; set; } = ownerId;
    public CappedLaptopManifestStatus Status { get; set; } = status;

    /// <summary>Starts a capped laptop manifest in status planned.</summary>
    public static CappedLaptopManifest NewCappedLaptopManifest(string code, string name)
    {
        return new CappedLaptopManifest(code, name, 0, false, 0, 0, "", "", CappedLaptopManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLaptopManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLaptopManifestStatus next)
    {
        switch (Status)
        {
            case CappedLaptopManifestStatus.Planned:
                switch (next)
                {
                    case CappedLaptopManifestStatus.Scheduled:
                    case CappedLaptopManifestStatus.InProgress:
                        return true;
                }
                return false;
            case CappedLaptopManifestStatus.Scheduled:
                switch (next)
                {
                    case CappedLaptopManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedLaptopManifestStatus.InProgress:
                switch (next)
                {
                    case CappedLaptopManifestStatus.Completed:
                    case CappedLaptopManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedLaptopManifestStatus.Completed:
                return next == CappedLaptopManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedLaptopManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped laptop manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped laptop manifest"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)UpdatedAt * (double)DurationMinutes, 2);
    }

    public CappedLaptopManifest Copy()
    {
        return new CappedLaptopManifest(
            Code,
            Name,
            Sequence,
            Approved,
            UpdatedAt,
            DurationMinutes,
            Reviewer,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
