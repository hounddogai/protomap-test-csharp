namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global scenario slot is in its life cycle.</summary>
public enum GlobalScenarioSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One global scenario slot, with what the planners need to act on it.</summary>
public sealed class GlobalScenarioSlot(string code, string label, int leaveDays, string externalRef, int capacity, int updatedAt, string phone, Dictionary<string, string> attributes, double cost, int windowDays, bool pinned, GlobalScenarioSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string ExternalRef { get; set; } = externalRef;
    public int Capacity { get; set; } = capacity;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Phone { get; set; } = phone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public int WindowDays { get; set; } = windowDays;
    public bool Pinned { get; set; } = pinned;
    public GlobalScenarioSlotStatus Status { get; set; } = status;

    /// <summary>Starts a global scenario slot in status new.</summary>
    public static GlobalScenarioSlot NewGlobalScenarioSlot(string code, string label)
    {
        return new GlobalScenarioSlot(
            code,
            label,
            0,
            "",
            0,
            0,
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            false,
            GlobalScenarioSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalScenarioSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalScenarioSlotStatus next)
    {
        switch (Status)
        {
            case GlobalScenarioSlotStatus.New:
                return next == GlobalScenarioSlotStatus.Queued || next == GlobalScenarioSlotStatus.Done;
            case GlobalScenarioSlotStatus.Queued:
                return next == GlobalScenarioSlotStatus.Running || next == GlobalScenarioSlotStatus.Done;
            case GlobalScenarioSlotStatus.Running:
                switch (next)
                {
                    case GlobalScenarioSlotStatus.Done:
                        return true;
                }
                return false;
            case GlobalScenarioSlotStatus.Done:
                switch (next)
                {
                    case GlobalScenarioSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalScenarioSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global scenario slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global scenario slot"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)Capacity + (double)LeaveDays, 2);
    }

    public GlobalScenarioSlot Copy()
    {
        return new GlobalScenarioSlot(
            Code,
            Label,
            LeaveDays,
            ExternalRef,
            Capacity,
            UpdatedAt,
            Phone,
            Attributes,
            Cost,
            WindowDays,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
