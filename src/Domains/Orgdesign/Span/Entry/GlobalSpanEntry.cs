namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global span entry is in its life cycle.</summary>
public enum GlobalSpanEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global span entry. Its code is unique within a tenant.</summary>
public sealed class GlobalSpanEntry(string code, string name, double floorValue, int attempts, double discount, string description, double fte, int sequence, string region, GlobalSpanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int Attempts { get; set; } = attempts;
    public double Discount { get; set; } = discount;
    public string Description { get; set; } = description;
    public double Fte { get; set; } = fte;
    public int Sequence { get; set; } = sequence;
    public string Region { get; set; } = region;
    public GlobalSpanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a global span entry in status planned.</summary>
    public static GlobalSpanEntry NewGlobalSpanEntry(string code, string name)
    {
        return new GlobalSpanEntry(code, name, 0.0, 0, 0.0, "", 0.0, 0, "", GlobalSpanEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSpanEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSpanEntryStatus next)
    {
        switch (Status)
        {
            case GlobalSpanEntryStatus.Planned:
                switch (next)
                {
                    case GlobalSpanEntryStatus.Completed:
                        return true;
                }
                return false;
            case GlobalSpanEntryStatus.Scheduled:
                switch (next)
                {
                    case GlobalSpanEntryStatus.Completed:
                    case GlobalSpanEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case GlobalSpanEntryStatus.InProgress:
                switch (next)
                {
                    case GlobalSpanEntryStatus.Completed:
                        return true;
                }
                return false;
            case GlobalSpanEntryStatus.Completed:
                return next == GlobalSpanEntryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalSpanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global span entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Sequence - Fte, 2);
    }

    public GlobalSpanEntry Copy()
    {
        return new GlobalSpanEntry(
            Code,
            Name,
            FloorValue,
            Attempts,
            Discount,
            Description,
            Fte,
            Sequence,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
