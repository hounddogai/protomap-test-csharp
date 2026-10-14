namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive town hall manifest is in its life cycle.</summary>
public enum AdaptiveTownHallManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One adaptive town hall manifest, with what the planners need to act on it.</summary>
public sealed class AdaptiveTownHallManifest(string code, string name, string team, string location, double rating, int quantity, int version, double margin, string label, AdaptiveTownHallManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Location { get; set; } = location;
    public double Rating { get; set; } = rating;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public double Margin { get; set; } = margin;
    public string Label { get; set; } = label;
    public AdaptiveTownHallManifestStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive town hall manifest in status proposed.</summary>
    public static AdaptiveTownHallManifest NewAdaptiveTownHallManifest(string code, string name)
    {
        return new AdaptiveTownHallManifest(
            code,
            name,
            "",
            "",
            0.0,
            0,
            0,
            0.0,
            "",
            AdaptiveTownHallManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTownHallManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTownHallManifestStatus next)
    {
        switch (Status)
        {
            case AdaptiveTownHallManifestStatus.Proposed:
                switch (next)
                {
                    case AdaptiveTownHallManifestStatus.Approved:
                        return true;
                }
                return false;
            case AdaptiveTownHallManifestStatus.Approved:
                return next == AdaptiveTownHallManifestStatus.Withdrawn;
            case AdaptiveTownHallManifestStatus.Rejected:
                switch (next)
                {
                    case AdaptiveTownHallManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveTownHallManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive town hall manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive town hall manifest"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Quantity * Margin, 2);
    }

    public AdaptiveTownHallManifest Copy()
    {
        return new AdaptiveTownHallManifest(
            Code,
            Name,
            Team,
            Location,
            Rating,
            Quantity,
            Version,
            Margin,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
