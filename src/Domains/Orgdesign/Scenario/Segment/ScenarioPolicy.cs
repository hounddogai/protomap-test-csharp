namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario policy is in its life cycle.</summary>
public enum ScenarioPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One scenario policy, with what the planners need to act on it.</summary>
public sealed class ScenarioPolicy(string code, string name, double amount, int updatedAt, int capacity, string team, int version, bool visible, ScenarioPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public ScenarioPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a scenario policy in status idle.</summary>
    public static ScenarioPolicy NewScenarioPolicy(string code, string name)
    {
        return new ScenarioPolicy(code, name, 0.0, 0, 0, "", 0, false, ScenarioPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioPolicyStatus next)
    {
        switch (Status)
        {
            case ScenarioPolicyStatus.Idle:
                switch (next)
                {
                    case ScenarioPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioPolicyStatus.Warming:
                return next == ScenarioPolicyStatus.Ready;
            case ScenarioPolicyStatus.Ready:
                switch (next)
                {
                    case ScenarioPolicyStatus.Draining:
                    case ScenarioPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioPolicyStatus.Draining:
                switch (next)
                {
                    case ScenarioPolicyStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)UpdatedAt + (double)Capacity, 2);
    }

    public ScenarioPolicy Copy()
    {
        return new ScenarioPolicy(Code, Name, Amount, UpdatedAt, Capacity, Team, Version, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
