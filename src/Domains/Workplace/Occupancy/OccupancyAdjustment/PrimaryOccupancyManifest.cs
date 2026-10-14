namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary occupancy manifest is in its life cycle.</summary>
public enum PrimaryOccupancyManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary occupancy manifest. Its code is unique within a tenant.</summary>
public sealed class PrimaryOccupancyManifest(string code, string name, string notes, int leaveDays, string timezone, string source, string team, string ownerId, string phone, string segment, PrimaryOccupancyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public int LeaveDays { get; set; } = leaveDays;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public string Team { get; set; } = team;
    public string OwnerId { get; set; } = ownerId;
    public string Phone { get; set; } = phone;
    public string Segment { get; set; } = segment;
    public PrimaryOccupancyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a primary occupancy manifest in status idle.</summary>
    public static PrimaryOccupancyManifest NewPrimaryOccupancyManifest(string code, string name)
    {
        return new PrimaryOccupancyManifest(
            code,
            name,
            "",
            0,
            "",
            "",
            "",
            "",
            "",
            "",
            PrimaryOccupancyManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryOccupancyManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryOccupancyManifestStatus next)
    {
        switch (Status)
        {
            case PrimaryOccupancyManifestStatus.Idle:
                return next == PrimaryOccupancyManifestStatus.Draining;
            case PrimaryOccupancyManifestStatus.Warming:
                return next == PrimaryOccupancyManifestStatus.Ready || next == PrimaryOccupancyManifestStatus.Stopped;
            case PrimaryOccupancyManifestStatus.Ready:
                return next == PrimaryOccupancyManifestStatus.Draining || next == PrimaryOccupancyManifestStatus.Stopped;
            case PrimaryOccupancyManifestStatus.Draining:
                switch (next)
                {
                    case PrimaryOccupancyManifestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryOccupancyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary occupancy manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary occupancy manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public PrimaryOccupancyManifest Copy()
    {
        return new PrimaryOccupancyManifest(
            Code,
            Name,
            Notes,
            LeaveDays,
            Timezone,
            Source,
            Team,
            OwnerId,
            Phone,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
