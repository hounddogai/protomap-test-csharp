namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling position manifest is in its life cycle.</summary>
public enum RollingPositionManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a rolling position manifest and its current status.</summary>
public sealed class RollingPositionManifest(string code, string name, double floorValue, string segment, string reviewer, int priority, RollingPositionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Segment { get; set; } = segment;
    public string Reviewer { get; set; } = reviewer;
    public int Priority { get; set; } = priority;
    public RollingPositionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling position manifest in status proposed.</summary>
    public static RollingPositionManifest NewRollingPositionManifest(string code, string name)
    {
        return new RollingPositionManifest(code, name, 0.0, "", "", 0, RollingPositionManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPositionManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPositionManifestStatus next)
    {
        switch (Status)
        {
            case RollingPositionManifestStatus.Proposed:
                return next == RollingPositionManifestStatus.Approved || next == RollingPositionManifestStatus.Rejected;
            case RollingPositionManifestStatus.Approved:
                switch (next)
                {
                    case RollingPositionManifestStatus.Rejected:
                    case RollingPositionManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingPositionManifestStatus.Rejected:
                return next == RollingPositionManifestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingPositionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling position manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Priority + FloorValue, 2);
    }

    public RollingPositionManifest Copy()
    {
        return new RollingPositionManifest(Code, Name, FloorValue, Segment, Reviewer, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
