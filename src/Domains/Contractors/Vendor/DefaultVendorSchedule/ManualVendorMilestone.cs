namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual vendor milestone is in its life cycle.</summary>
public enum ManualVendorMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual vendor milestone. Its code is unique within a tenant.</summary>
public sealed class ManualVendorMilestone(string code, string name, string description, int sequence, int version, double margin, int createdAt, bool approved, string checksum, double minimum, ManualVendorMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Sequence { get; set; } = sequence;
    public int Version { get; set; } = version;
    public double Margin { get; set; } = margin;
    public int CreatedAt { get; set; } = createdAt;
    public bool Approved { get; set; } = approved;
    public string Checksum { get; set; } = checksum;
    public double Minimum { get; set; } = minimum;
    public ManualVendorMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a manual vendor milestone in status idle.</summary>
    public static ManualVendorMilestone NewManualVendorMilestone(string code, string name)
    {
        return new ManualVendorMilestone(
            code,
            name,
            "",
            0,
            0,
            0.0,
            0,
            false,
            "",
            0.0,
            ManualVendorMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualVendorMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualVendorMilestoneStatus next)
    {
        switch (Status)
        {
            case ManualVendorMilestoneStatus.Idle:
                switch (next)
                {
                    case ManualVendorMilestoneStatus.Ready:
                    case ManualVendorMilestoneStatus.Draining:
                        return true;
                }
                return false;
            case ManualVendorMilestoneStatus.Warming:
                switch (next)
                {
                    case ManualVendorMilestoneStatus.Draining:
                    case ManualVendorMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case ManualVendorMilestoneStatus.Ready:
                switch (next)
                {
                    case ManualVendorMilestoneStatus.Draining:
                        return true;
                }
                return false;
            case ManualVendorMilestoneStatus.Draining:
                switch (next)
                {
                    case ManualVendorMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualVendorMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual vendor milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"manual vendor milestone"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Version * Minimum, 2);
    }

    public ManualVendorMilestone Copy()
    {
        return new ManualVendorMilestone(
            Code,
            Name,
            Description,
            Sequence,
            Version,
            Margin,
            CreatedAt,
            Approved,
            Checksum,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
