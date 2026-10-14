namespace Acme.Workforce.Orgdesign.Scenario.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending scenario checkpoint is in its life cycle.</summary>
public enum PendingScenarioCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending scenario checkpoint as the scenario team keeps it.</summary>
public sealed class PendingScenarioCheckpoint(string code, string label, int capacity, int endsAt, double floorValue, string country, Dictionary<string, string> attributes, string team, double fte, double threshold, PendingScenarioCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public int EndsAt { get; set; } = endsAt;
    public double FloorValue { get; set; } = floorValue;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public double Fte { get; set; } = fte;
    public double Threshold { get; set; } = threshold;
    public PendingScenarioCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a pending scenario checkpoint in status proposed.</summary>
    public static PendingScenarioCheckpoint NewPendingScenarioCheckpoint(string code, string label)
    {
        return new PendingScenarioCheckpoint(
            code,
            label,
            0,
            0,
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            0.0,
            PendingScenarioCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingScenarioCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingScenarioCheckpointStatus next)
    {
        switch (Status)
        {
            case PendingScenarioCheckpointStatus.Proposed:
                switch (next)
                {
                    case PendingScenarioCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingScenarioCheckpointStatus.Approved:
                return next == PendingScenarioCheckpointStatus.Rejected;
            case PendingScenarioCheckpointStatus.Rejected:
                switch (next)
                {
                    case PendingScenarioCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingScenarioCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending scenario checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double WeightedTotal()
    {
        return Math.Round((double)EndsAt - (double)Capacity, 2);
    }

    public PendingScenarioCheckpoint Copy()
    {
        return new PendingScenarioCheckpoint(
            Code,
            Label,
            Capacity,
            EndsAt,
            FloorValue,
            Country,
            Attributes,
            Team,
            Fte,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
