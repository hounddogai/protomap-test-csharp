namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario checkpoint is in its life cycle.</summary>
public enum ScenarioCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scenario checkpoint as the scenario team keeps it.</summary>
public sealed class ScenarioCheckpoint(string code, string title, int createdAt, int retries, double discount, string externalRef, string description, bool active, string currency, ScenarioCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int Retries { get; set; } = retries;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public string Description { get; set; } = description;
    public bool Active { get; set; } = active;
    public string Currency { get; set; } = currency;
    public ScenarioCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a scenario checkpoint in status idle.</summary>
    public static ScenarioCheckpoint NewScenarioCheckpoint(string code, string title)
    {
        return new ScenarioCheckpoint(code, title, 0, 0, 0.0, "", "", false, "", ScenarioCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioCheckpointStatus next)
    {
        switch (Status)
        {
            case ScenarioCheckpointStatus.Idle:
                switch (next)
                {
                    case ScenarioCheckpointStatus.Ready:
                    case ScenarioCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioCheckpointStatus.Warming:
                switch (next)
                {
                    case ScenarioCheckpointStatus.Ready:
                        return true;
                }
                return false;
            case ScenarioCheckpointStatus.Ready:
                return next == ScenarioCheckpointStatus.Draining || next == ScenarioCheckpointStatus.Stopped;
            case ScenarioCheckpointStatus.Draining:
                return next == ScenarioCheckpointStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScenarioCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Retries - Discount, 2);
    }

    public ScenarioCheckpoint Copy()
    {
        return new ScenarioCheckpoint(
            Code,
            Title,
            CreatedAt,
            Retries,
            Discount,
            ExternalRef,
            Description,
            Active,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
