namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat override is in its life cycle.</summary>
public enum LicenseSeatOverrideStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a license seat override and its current status.</summary>
public sealed class LicenseSeatOverride(string code, string label, string category, bool active, string team, double threshold, string checksum, Dictionary<string, string> attributes, double floorValue, string unit, int version, LicenseSeatOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public string Team { get; set; } = team;
    public double Threshold { get; set; } = threshold;
    public string Checksum { get; set; } = checksum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double FloorValue { get; set; } = floorValue;
    public string Unit { get; set; } = unit;
    public int Version { get; set; } = version;
    public LicenseSeatOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a license seat override in status idle.</summary>
    public static LicenseSeatOverride NewLicenseSeatOverride(string code, string label)
    {
        return new LicenseSeatOverride(
            code,
            label,
            "",
            false,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            0,
            LicenseSeatOverrideStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatOverrideStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatOverrideStatus next)
    {
        switch (Status)
        {
            case LicenseSeatOverrideStatus.Idle:
                switch (next)
                {
                    case LicenseSeatOverrideStatus.Stopped:
                        return true;
                }
                return false;
            case LicenseSeatOverrideStatus.Warming:
                return next == LicenseSeatOverrideStatus.Ready || next == LicenseSeatOverrideStatus.Stopped;
            case LicenseSeatOverrideStatus.Ready:
                return next == LicenseSeatOverrideStatus.Draining;
            case LicenseSeatOverrideStatus.Draining:
                return next == LicenseSeatOverrideStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LicenseSeatOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Version + FloorValue, 2);
    }

    public LicenseSeatOverride Copy()
    {
        return new LicenseSeatOverride(
            Code,
            Label,
            Category,
            Active,
            Team,
            Threshold,
            Checksum,
            Attributes,
            FloorValue,
            Unit,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
