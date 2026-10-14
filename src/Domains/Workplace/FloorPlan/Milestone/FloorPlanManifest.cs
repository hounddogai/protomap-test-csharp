namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan manifest is in its life cycle.</summary>
public enum FloorPlanManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a floor plan manifest and its current status.</summary>
public sealed class FloorPlanManifest(string code, string title, int version, int createdAt, string employeeId, string checksum, double weight, string region, int limit, FloorPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public string EmployeeId { get; set; } = employeeId;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public string Region { get; set; } = region;
    public int Limit { get; set; } = limit;
    public FloorPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan manifest in status proposed.</summary>
    public static FloorPlanManifest NewFloorPlanManifest(string code, string title)
    {
        return new FloorPlanManifest(code, title, 0, 0, "", "", 0.0, "", 0, FloorPlanManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanManifestStatus next)
    {
        switch (Status)
        {
            case FloorPlanManifestStatus.Proposed:
                switch (next)
                {
                    case FloorPlanManifestStatus.Rejected:
                    case FloorPlanManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case FloorPlanManifestStatus.Approved:
                return next == FloorPlanManifestStatus.Rejected;
            case FloorPlanManifestStatus.Rejected:
                switch (next)
                {
                    case FloorPlanManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)CreatedAt - (double)Limit, 2);
    }

    public FloorPlanManifest Copy()
    {
        return new FloorPlanManifest(
            Code,
            Title,
            Version,
            CreatedAt,
            EmployeeId,
            Checksum,
            Weight,
            Region,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
