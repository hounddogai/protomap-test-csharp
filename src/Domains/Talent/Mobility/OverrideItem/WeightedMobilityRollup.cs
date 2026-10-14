namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted mobility rollup is in its life cycle.</summary>
public enum WeightedMobilityRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a weighted mobility rollup and its current status.</summary>
public sealed class WeightedMobilityRollup(string code, string name, string contactEmail, bool archived, string description, double threshold, int version, WeightedMobilityRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public int Version { get; set; } = version;
    public WeightedMobilityRollupStatus Status { get; set; } = status;

    /// <summary>Starts a weighted mobility rollup in status created.</summary>
    public static WeightedMobilityRollup NewWeightedMobilityRollup(string code, string name)
    {
        return new WeightedMobilityRollup(code, name, "", false, "", 0.0, 0, WeightedMobilityRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedMobilityRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedMobilityRollupStatus next)
    {
        switch (Status)
        {
            case WeightedMobilityRollupStatus.Created:
                return next == WeightedMobilityRollupStatus.Retired;
            case WeightedMobilityRollupStatus.Validated:
                return next == WeightedMobilityRollupStatus.Published || next == WeightedMobilityRollupStatus.Retired;
            case WeightedMobilityRollupStatus.Published:
                switch (next)
                {
                    case WeightedMobilityRollupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedMobilityRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted mobility rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold + (double)Version, 2);
    }

    public WeightedMobilityRollup Copy()
    {
        return new WeightedMobilityRollup(
            Code,
            Name,
            ContactEmail,
            Archived,
            Description,
            Threshold,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
