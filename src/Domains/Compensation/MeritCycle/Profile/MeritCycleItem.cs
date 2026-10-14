namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle item is in its life cycle.</summary>
public enum MeritCycleItemStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One merit cycle item, with what the planners need to act on it.</summary>
public sealed class MeritCycleItem(string code, string label, int durationMinutes, Dictionary<string, string> attributes, int priority, double ceiling, MeritCycleItemStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Priority { get; set; } = priority;
    public double Ceiling { get; set; } = ceiling;
    public MeritCycleItemStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle item in status planned.</summary>
    public static MeritCycleItem NewMeritCycleItem(string code, string label)
    {
        return new MeritCycleItem(
            code,
            label,
            0,
            new Dictionary<string, string>(),
            0,
            0.0,
            MeritCycleItemStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleItemStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleItemStatus next)
    {
        switch (Status)
        {
            case MeritCycleItemStatus.Planned:
                switch (next)
                {
                    case MeritCycleItemStatus.Scheduled:
                    case MeritCycleItemStatus.Completed:
                        return true;
                }
                return false;
            case MeritCycleItemStatus.Scheduled:
                return next == MeritCycleItemStatus.InProgress || next == MeritCycleItemStatus.Cancelled;
            case MeritCycleItemStatus.InProgress:
                return next == MeritCycleItemStatus.Cancelled;
            case MeritCycleItemStatus.Completed:
                switch (next)
                {
                    case MeritCycleItemStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double ProjectedValue()
    {
        return Math.Round((double)DurationMinutes + (double)Priority, 2);
    }

    public MeritCycleItem Copy()
    {
        return new MeritCycleItem(Code, Label, DurationMinutes, Attributes, Priority, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
