namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled desk batch is in its life cycle.</summary>
public enum ScheduledDeskBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled desk batch as the desk team keeps it.</summary>
public sealed class ScheduledDeskBatch(string code, string label, Dictionary<string, string> attributes, string region, int windowDays, double budget, double fte, int createdAt, ScheduledDeskBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Region { get; set; } = region;
    public int WindowDays { get; set; } = windowDays;
    public double Budget { get; set; } = budget;
    public double Fte { get; set; } = fte;
    public int CreatedAt { get; set; } = createdAt;
    public ScheduledDeskBatchStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled desk batch in status planned.</summary>
    public static ScheduledDeskBatch NewScheduledDeskBatch(string code, string label)
    {
        return new ScheduledDeskBatch(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            0.0,
            0,
            ScheduledDeskBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledDeskBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledDeskBatchStatus next)
    {
        switch (Status)
        {
            case ScheduledDeskBatchStatus.Planned:
                return next == ScheduledDeskBatchStatus.InProgress;
            case ScheduledDeskBatchStatus.Scheduled:
                switch (next)
                {
                    case ScheduledDeskBatchStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledDeskBatchStatus.InProgress:
                switch (next)
                {
                    case ScheduledDeskBatchStatus.Completed:
                    case ScheduledDeskBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledDeskBatchStatus.Completed:
                switch (next)
                {
                    case ScheduledDeskBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledDeskBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled desk batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double NetLoad()
    {
        return Math.Round((double)WindowDays + Budget, 2);
    }

    public ScheduledDeskBatch Copy()
    {
        return new ScheduledDeskBatch(Code, Label, Attributes, Region, WindowDays, Budget, Fte, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
