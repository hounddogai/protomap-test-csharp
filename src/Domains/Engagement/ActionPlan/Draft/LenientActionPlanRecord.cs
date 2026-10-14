namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient action plan record is in its life cycle.</summary>
public enum LenientActionPlanRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient action plan record. Its code is unique within a tenant.</summary>
public sealed class LenientActionPlanRecord(string code, string name, string checksum, bool approved, string phone, int capacity, double hours, bool active, double fte, int sequence, string summary, LenientActionPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public bool Approved { get; set; } = approved;
    public string Phone { get; set; } = phone;
    public int Capacity { get; set; } = capacity;
    public double Hours { get; set; } = hours;
    public bool Active { get; set; } = active;
    public double Fte { get; set; } = fte;
    public int Sequence { get; set; } = sequence;
    public string Summary { get; set; } = summary;
    public LenientActionPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a lenient action plan record in status planned.</summary>
    public static LenientActionPlanRecord NewLenientActionPlanRecord(string code, string name)
    {
        return new LenientActionPlanRecord(
            code,
            name,
            "",
            false,
            "",
            0,
            0.0,
            false,
            0.0,
            0,
            "",
            LenientActionPlanRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientActionPlanRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientActionPlanRecordStatus next)
    {
        switch (Status)
        {
            case LenientActionPlanRecordStatus.Planned:
                switch (next)
                {
                    case LenientActionPlanRecordStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientActionPlanRecordStatus.Scheduled:
                switch (next)
                {
                    case LenientActionPlanRecordStatus.InProgress:
                        return true;
                }
                return false;
            case LenientActionPlanRecordStatus.InProgress:
                return next == LenientActionPlanRecordStatus.Cancelled;
            case LenientActionPlanRecordStatus.Completed:
                switch (next)
                {
                    case LenientActionPlanRecordStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientActionPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient action plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Sequence + Hours, 2);
    }

    public LenientActionPlanRecord Copy()
    {
        return new LenientActionPlanRecord(
            Code,
            Name,
            Checksum,
            Approved,
            Phone,
            Capacity,
            Hours,
            Active,
            Fte,
            Sequence,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
