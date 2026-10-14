namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario slot is in its life cycle.</summary>
public enum ScenarioSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One scenario slot, with what the planners need to act on it.</summary>
public sealed class ScenarioSlot(string code, string name, string unit, Dictionary<string, string> attributes, int attempts, string title, string salaryBand, int tenureMonths, double minimum, ScenarioSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Minimum { get; set; } = minimum;
    public ScenarioSlotStatus Status { get; set; } = status;

    /// <summary>Starts a scenario slot in status open.</summary>
    public static ScenarioSlot NewScenarioSlot(string code, string name)
    {
        return new ScenarioSlot(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            0,
            "",
            "",
            0,
            0.0,
            ScenarioSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioSlotStatus next)
    {
        switch (Status)
        {
            case ScenarioSlotStatus.Open:
                switch (next)
                {
                    case ScenarioSlotStatus.Archived:
                        return true;
                }
                return false;
            case ScenarioSlotStatus.InReview:
                return next == ScenarioSlotStatus.Resolved;
            case ScenarioSlotStatus.Resolved:
                return next == ScenarioSlotStatus.Archived;
            case ScenarioSlotStatus.Reopened:
                return next == ScenarioSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScenarioSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double NetVolume()
    {
        return Math.Round((double)TenureMonths - (double)Attempts, 2);
    }

    public ScenarioSlot Copy()
    {
        return new ScenarioSlot(
            Code,
            Name,
            Unit,
            Attributes,
            Attempts,
            Title,
            SalaryBand,
            TenureMonths,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
