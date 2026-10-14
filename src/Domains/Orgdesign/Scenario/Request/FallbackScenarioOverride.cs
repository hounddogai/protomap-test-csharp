namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback scenario override is in its life cycle.</summary>
public enum FallbackScenarioOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback scenario override as the scenario team keeps it.</summary>
public sealed class FallbackScenarioOverride(string code, string name, int revision, string location, string reviewer, bool archived, double threshold, int batchSize, double weight, string region, FallbackScenarioOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public bool Archived { get; set; } = archived;
    public double Threshold { get; set; } = threshold;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public string Region { get; set; } = region;
    public FallbackScenarioOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a fallback scenario override in status requested.</summary>
    public static FallbackScenarioOverride NewFallbackScenarioOverride(string code, string name)
    {
        return new FallbackScenarioOverride(
            code,
            name,
            0,
            "",
            "",
            false,
            0.0,
            0,
            0.0,
            "",
            FallbackScenarioOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackScenarioOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackScenarioOverrideStatus next)
    {
        switch (Status)
        {
            case FallbackScenarioOverrideStatus.Requested:
                switch (next)
                {
                    case FallbackScenarioOverrideStatus.Granted:
                    case FallbackScenarioOverrideStatus.Expired:
                        return true;
                }
                return false;
            case FallbackScenarioOverrideStatus.Granted:
                switch (next)
                {
                    case FallbackScenarioOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackScenarioOverrideStatus.Revoked:
                switch (next)
                {
                    case FallbackScenarioOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackScenarioOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback scenario override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetLoad()
    {
        return Math.Round(Weight * (double)BatchSize, 2);
    }

    public FallbackScenarioOverride Copy()
    {
        return new FallbackScenarioOverride(
            Code,
            Name,
            Revision,
            Location,
            Reviewer,
            Archived,
            Threshold,
            BatchSize,
            Weight,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
