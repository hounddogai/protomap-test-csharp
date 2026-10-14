namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted town hall manifest is in its life cycle.</summary>
public enum WeightedTownHallManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a weighted town hall manifest and its current status.</summary>
public sealed class WeightedTownHallManifest(string code, string name, int version, int durationMinutes, double maximum, double cost, string summary, WeightedTownHallManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public string Summary { get; set; } = summary;
    public WeightedTownHallManifestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted town hall manifest in status created.</summary>
    public static WeightedTownHallManifest NewWeightedTownHallManifest(string code, string name)
    {
        return new WeightedTownHallManifest(code, name, 0, 0, 0.0, 0.0, "", WeightedTownHallManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTownHallManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTownHallManifestStatus next)
    {
        switch (Status)
        {
            case WeightedTownHallManifestStatus.Created:
                return next == WeightedTownHallManifestStatus.Published;
            case WeightedTownHallManifestStatus.Validated:
                return next == WeightedTownHallManifestStatus.Published || next == WeightedTownHallManifestStatus.Retired;
            case WeightedTownHallManifestStatus.Published:
                switch (next)
                {
                    case WeightedTownHallManifestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedTownHallManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted town hall manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Version + Maximum, 2);
    }

    public WeightedTownHallManifest Copy()
    {
        return new WeightedTownHallManifest(Code, Name, Version, DurationMinutes, Maximum, Cost, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
