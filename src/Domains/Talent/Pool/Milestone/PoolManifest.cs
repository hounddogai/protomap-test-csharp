namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool manifest is in its life cycle.</summary>
public enum PoolManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pool manifest and its current status.</summary>
public sealed class PoolManifest(string code, string label, string location, int windowDays, int version, string description, string notes, string salaryBand, double floorValue, string region, List<string> tags, PoolManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public int WindowDays { get; set; } = windowDays;
    public int Version { get; set; } = version;
    public string Description { get; set; } = description;
    public string Notes { get; set; } = notes;
    public string SalaryBand { get; set; } = salaryBand;
    public double FloorValue { get; set; } = floorValue;
    public string Region { get; set; } = region;
    public List<string> Tags { get; set; } = tags;
    public PoolManifestStatus Status { get; set; } = status;

    /// <summary>Starts a pool manifest in status proposed.</summary>
    public static PoolManifest NewPoolManifest(string code, string label)
    {
        return new PoolManifest(
            code,
            label,
            "",
            0,
            0,
            "",
            "",
            "",
            0.0,
            "",
            new List<string>(),
            PoolManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolManifestStatus next)
    {
        switch (Status)
        {
            case PoolManifestStatus.Proposed:
                switch (next)
                {
                    case PoolManifestStatus.Rejected:
                    case PoolManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolManifestStatus.Approved:
                switch (next)
                {
                    case PoolManifestStatus.Rejected:
                    case PoolManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolManifestStatus.Rejected:
                switch (next)
                {
                    case PoolManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)WindowDays + (double)Version, 2);
    }

    public PoolManifest Copy()
    {
        return new PoolManifest(
            Code,
            Label,
            Location,
            WindowDays,
            Version,
            Description,
            Notes,
            SalaryBand,
            FloorValue,
            Region,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
