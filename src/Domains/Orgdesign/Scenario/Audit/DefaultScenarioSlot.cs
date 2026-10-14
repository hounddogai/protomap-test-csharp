namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default scenario slot is in its life cycle.</summary>
public enum DefaultScenarioSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default scenario slot and its current status.</summary>
public sealed class DefaultScenarioSlot(string code, string title, bool archived, double weight, string label, string unit, double ceiling, string ownerId, DefaultScenarioSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public double Weight { get; set; } = weight;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public string OwnerId { get; set; } = ownerId;
    public DefaultScenarioSlotStatus Status { get; set; } = status;

    /// <summary>Starts a default scenario slot in status proposed.</summary>
    public static DefaultScenarioSlot NewDefaultScenarioSlot(string code, string title)
    {
        return new DefaultScenarioSlot(code, title, false, 0.0, "", "", 0.0, "", DefaultScenarioSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultScenarioSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultScenarioSlotStatus next)
    {
        switch (Status)
        {
            case DefaultScenarioSlotStatus.Proposed:
                switch (next)
                {
                    case DefaultScenarioSlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultScenarioSlotStatus.Approved:
                switch (next)
                {
                    case DefaultScenarioSlotStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultScenarioSlotStatus.Rejected:
                return next == DefaultScenarioSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultScenarioSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default scenario slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Weight * Ceiling, 2);
    }

    public DefaultScenarioSlot Copy()
    {
        return new DefaultScenarioSlot(Code, Title, Archived, Weight, Label, Unit, Ceiling, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
