namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient engagement manifest is in its life cycle.</summary>
public enum LenientEngagementManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a lenient engagement manifest and its current status.</summary>
public sealed class LenientEngagementManifest(string code, string label, bool visible, string channel, string department, string unit, double maximum, string skill, string name, double cost, LenientEngagementManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Channel { get; set; } = channel;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public double Maximum { get; set; } = maximum;
    public string Skill { get; set; } = skill;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public LenientEngagementManifestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient engagement manifest in status created.</summary>
    public static LenientEngagementManifest NewLenientEngagementManifest(string code, string label)
    {
        return new LenientEngagementManifest(
            code,
            label,
            false,
            "",
            "",
            "",
            0.0,
            "",
            "",
            0.0,
            LenientEngagementManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientEngagementManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientEngagementManifestStatus next)
    {
        switch (Status)
        {
            case LenientEngagementManifestStatus.Created:
                switch (next)
                {
                    case LenientEngagementManifestStatus.Validated:
                    case LenientEngagementManifestStatus.Published:
                        return true;
                }
                return false;
            case LenientEngagementManifestStatus.Validated:
                return next == LenientEngagementManifestStatus.Retired;
            case LenientEngagementManifestStatus.Published:
                return next == LenientEngagementManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientEngagementManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient engagement manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Maximum - Cost, 2);
    }

    public LenientEngagementManifest Copy()
    {
        return new LenientEngagementManifest(
            Code,
            Label,
            Visible,
            Channel,
            Department,
            Unit,
            Maximum,
            Skill,
            Name,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
