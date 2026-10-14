namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal mobility manifest is in its life cycle.</summary>
public enum SeasonalMobilityManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal mobility manifest as the mobility team keeps it.</summary>
public sealed class SeasonalMobilityManifest(string code, string name, double budget, double ceiling, string unit, int priority, SeasonalMobilityManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public double Ceiling { get; set; } = ceiling;
    public string Unit { get; set; } = unit;
    public int Priority { get; set; } = priority;
    public SeasonalMobilityManifestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal mobility manifest in status requested.</summary>
    public static SeasonalMobilityManifest NewSeasonalMobilityManifest(string code, string name)
    {
        return new SeasonalMobilityManifest(code, name, 0.0, 0.0, "", 0, SeasonalMobilityManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalMobilityManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalMobilityManifestStatus next)
    {
        switch (Status)
        {
            case SeasonalMobilityManifestStatus.Requested:
                switch (next)
                {
                    case SeasonalMobilityManifestStatus.Granted:
                        return true;
                }
                return false;
            case SeasonalMobilityManifestStatus.Granted:
                switch (next)
                {
                    case SeasonalMobilityManifestStatus.Revoked:
                    case SeasonalMobilityManifestStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalMobilityManifestStatus.Revoked:
                switch (next)
                {
                    case SeasonalMobilityManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalMobilityManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal mobility manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal mobility manifest"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ceiling * Budget, 2);
    }

    public SeasonalMobilityManifest Copy()
    {
        return new SeasonalMobilityManifest(Code, Name, Budget, Ceiling, Unit, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
