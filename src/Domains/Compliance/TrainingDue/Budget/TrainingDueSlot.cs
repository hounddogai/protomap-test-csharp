namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due slot is in its life cycle.</summary>
public enum TrainingDueSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One training due slot, with what the planners need to act on it.</summary>
public sealed class TrainingDueSlot(string code, string label, string source, double cost, double ratio, string location, int attempts, bool active, Dictionary<string, string> attributes, string segment, string name, TrainingDueSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Cost { get; set; } = cost;
    public double Ratio { get; set; } = ratio;
    public string Location { get; set; } = location;
    public int Attempts { get; set; } = attempts;
    public bool Active { get; set; } = active;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Segment { get; set; } = segment;
    public string Name { get; set; } = name;
    public TrainingDueSlotStatus Status { get; set; } = status;

    /// <summary>Starts a training due slot in status proposed.</summary>
    public static TrainingDueSlot NewTrainingDueSlot(string code, string label)
    {
        return new TrainingDueSlot(
            code,
            label,
            "",
            0.0,
            0.0,
            "",
            0,
            false,
            new Dictionary<string, string>(),
            "",
            "",
            TrainingDueSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueSlotStatus next)
    {
        switch (Status)
        {
            case TrainingDueSlotStatus.Proposed:
                return next == TrainingDueSlotStatus.Approved || next == TrainingDueSlotStatus.Rejected;
            case TrainingDueSlotStatus.Approved:
                switch (next)
                {
                    case TrainingDueSlotStatus.Rejected:
                    case TrainingDueSlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case TrainingDueSlotStatus.Rejected:
                return next == TrainingDueSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TrainingDueSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts * Ratio, 2);
    }

    public TrainingDueSlot Copy()
    {
        return new TrainingDueSlot(
            Code,
            Label,
            Source,
            Cost,
            Ratio,
            Location,
            Attempts,
            Active,
            Attributes,
            Segment,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
