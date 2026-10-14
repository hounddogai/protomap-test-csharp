namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary floor plan bundle is in its life cycle.</summary>
public enum PrimaryFloorPlanBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary floor plan bundle. Its code is unique within a tenant.</summary>
public sealed class PrimaryFloorPlanBundle(string code, string name, int capacity, double fte, string label, string checksum, string timezone, List<string> tags, PrimaryFloorPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Fte { get; set; } = fte;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public PrimaryFloorPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a primary floor plan bundle in status idle.</summary>
    public static PrimaryFloorPlanBundle NewPrimaryFloorPlanBundle(string code, string name)
    {
        return new PrimaryFloorPlanBundle(
            code,
            name,
            0,
            0.0,
            "",
            "",
            "",
            new List<string>(),
            PrimaryFloorPlanBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryFloorPlanBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryFloorPlanBundleStatus next)
    {
        switch (Status)
        {
            case PrimaryFloorPlanBundleStatus.Idle:
                switch (next)
                {
                    case PrimaryFloorPlanBundleStatus.Warming:
                        return true;
                }
                return false;
            case PrimaryFloorPlanBundleStatus.Warming:
                switch (next)
                {
                    case PrimaryFloorPlanBundleStatus.Draining:
                        return true;
                }
                return false;
            case PrimaryFloorPlanBundleStatus.Ready:
                switch (next)
                {
                    case PrimaryFloorPlanBundleStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryFloorPlanBundleStatus.Draining:
                return next == PrimaryFloorPlanBundleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PrimaryFloorPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary floor plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
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

    public double NetLoad()
    {
        return Math.Round((double)Capacity + Fte, 2);
    }

    public PrimaryFloorPlanBundle Copy()
    {
        return new PrimaryFloorPlanBundle(Code, Name, Capacity, Fte, Label, Checksum, Timezone, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
