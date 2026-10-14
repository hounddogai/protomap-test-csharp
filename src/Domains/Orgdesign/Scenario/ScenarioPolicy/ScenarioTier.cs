namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario tier is in its life cycle.</summary>
public enum ScenarioTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scenario tier. Its code is unique within a tenant.</summary>
public sealed class ScenarioTier(string code, string name, int revision, bool archived, double budget, string timezone, int windowDays, string unit, int endsAt, ScenarioTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public bool Archived { get; set; } = archived;
    public double Budget { get; set; } = budget;
    public string Timezone { get; set; } = timezone;
    public int WindowDays { get; set; } = windowDays;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public ScenarioTierStatus Status { get; set; } = status;

    /// <summary>Starts a scenario tier in status proposed.</summary>
    public static ScenarioTier NewScenarioTier(string code, string name)
    {
        return new ScenarioTier(code, name, 0, false, 0.0, "", 0, "", 0, ScenarioTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioTierStatus next)
    {
        switch (Status)
        {
            case ScenarioTierStatus.Proposed:
                return next == ScenarioTierStatus.Rejected;
            case ScenarioTierStatus.Approved:
                return next == ScenarioTierStatus.Rejected || next == ScenarioTierStatus.Withdrawn;
            case ScenarioTierStatus.Rejected:
                return next == ScenarioTierStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScenarioTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"scenario tier"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)WindowDays * (double)EndsAt, 2);
    }

    public ScenarioTier Copy()
    {
        return new ScenarioTier(Code, Name, Revision, Archived, Budget, Timezone, WindowDays, Unit, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
