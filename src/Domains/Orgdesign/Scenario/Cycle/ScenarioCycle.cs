namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario cycle is in its life cycle.</summary>
public enum ScenarioCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scenario cycle. Its code is unique within a tenant.</summary>
public sealed class ScenarioCycle(string code, string label, double floorValue, string country, int priority, int leaveDays, int quantity, int durationMinutes, string phone, bool approved, ScenarioCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Country { get; set; } = country;
    public int Priority { get; set; } = priority;
    public int LeaveDays { get; set; } = leaveDays;
    public int Quantity { get; set; } = quantity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public ScenarioCycleStatus Status { get; set; } = status;

    /// <summary>Starts a scenario cycle in status created.</summary>
    public static ScenarioCycle NewScenarioCycle(string code, string label)
    {
        return new ScenarioCycle(code, label, 0.0, "", 0, 0, 0, 0, "", false, ScenarioCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioCycleStatus next)
    {
        switch (Status)
        {
            case ScenarioCycleStatus.Created:
                return next == ScenarioCycleStatus.Published;
            case ScenarioCycleStatus.Validated:
                return next == ScenarioCycleStatus.Published;
            case ScenarioCycleStatus.Published:
                return next == ScenarioCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScenarioCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Quantity * (double)DurationMinutes, 2);
    }

    public ScenarioCycle Copy()
    {
        return new ScenarioCycle(
            Code,
            Label,
            FloorValue,
            Country,
            Priority,
            LeaveDays,
            Quantity,
            DurationMinutes,
            Phone,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
