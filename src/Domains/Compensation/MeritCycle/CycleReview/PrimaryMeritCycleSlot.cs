namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary merit cycle slot is in its life cycle.</summary>
public enum PrimaryMeritCycleSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary merit cycle slot as the merit cycle team keeps it.</summary>
public sealed class PrimaryMeritCycleSlot(string code, string name, string checksum, int sequence, List<string> tags, bool archived, PrimaryMeritCycleSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public int Sequence { get; set; } = sequence;
    public List<string> Tags { get; set; } = tags;
    public bool Archived { get; set; } = archived;
    public PrimaryMeritCycleSlotStatus Status { get; set; } = status;

    /// <summary>Starts a primary merit cycle slot in status planned.</summary>
    public static PrimaryMeritCycleSlot NewPrimaryMeritCycleSlot(string code, string name)
    {
        return new PrimaryMeritCycleSlot(
            code,
            name,
            "",
            0,
            new List<string>(),
            false,
            PrimaryMeritCycleSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryMeritCycleSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryMeritCycleSlotStatus next)
    {
        switch (Status)
        {
            case PrimaryMeritCycleSlotStatus.Planned:
                switch (next)
                {
                    case PrimaryMeritCycleSlotStatus.Scheduled:
                    case PrimaryMeritCycleSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryMeritCycleSlotStatus.Scheduled:
                switch (next)
                {
                    case PrimaryMeritCycleSlotStatus.InProgress:
                    case PrimaryMeritCycleSlotStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryMeritCycleSlotStatus.InProgress:
                switch (next)
                {
                    case PrimaryMeritCycleSlotStatus.Completed:
                    case PrimaryMeritCycleSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryMeritCycleSlotStatus.Completed:
                switch (next)
                {
                    case PrimaryMeritCycleSlotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryMeritCycleSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary merit cycle slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public PrimaryMeritCycleSlot Copy()
    {
        return new PrimaryMeritCycleSlot(Code, Name, Checksum, Sequence, Tags, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
