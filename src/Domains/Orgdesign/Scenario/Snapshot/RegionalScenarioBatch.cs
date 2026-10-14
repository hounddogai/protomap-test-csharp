namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional scenario batch is in its life cycle.</summary>
public enum RegionalScenarioBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional scenario batch. Its code is unique within a tenant.</summary>
public sealed class RegionalScenarioBatch(string code, string label, int retries, List<string> tags, double maximum, int capacity, string currency, string region, int durationMinutes, int batchSize, string name, RegionalScenarioBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public double Maximum { get; set; } = maximum;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public string Region { get; set; } = region;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int BatchSize { get; set; } = batchSize;
    public string Name { get; set; } = name;
    public RegionalScenarioBatchStatus Status { get; set; } = status;

    /// <summary>Starts a regional scenario batch in status created.</summary>
    public static RegionalScenarioBatch NewRegionalScenarioBatch(string code, string label)
    {
        return new RegionalScenarioBatch(
            code,
            label,
            0,
            new List<string>(),
            0.0,
            0,
            "",
            "",
            0,
            0,
            "",
            RegionalScenarioBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalScenarioBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalScenarioBatchStatus next)
    {
        switch (Status)
        {
            case RegionalScenarioBatchStatus.Created:
                return next == RegionalScenarioBatchStatus.Published;
            case RegionalScenarioBatchStatus.Validated:
                return next == RegionalScenarioBatchStatus.Published || next == RegionalScenarioBatchStatus.Retired;
            case RegionalScenarioBatchStatus.Published:
                switch (next)
                {
                    case RegionalScenarioBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalScenarioBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional scenario batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional scenario batch"} {Code}";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Retries - Maximum, 2);
    }

    public RegionalScenarioBatch Copy()
    {
        return new RegionalScenarioBatch(
            Code,
            Label,
            Retries,
            Tags,
            Maximum,
            Capacity,
            Currency,
            Region,
            DurationMinutes,
            BatchSize,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
