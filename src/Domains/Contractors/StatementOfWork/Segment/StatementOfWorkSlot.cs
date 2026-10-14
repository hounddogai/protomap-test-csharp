namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work slot is in its life cycle.</summary>
public enum StatementOfWorkSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A statement of work slot. Its code is unique within a tenant.</summary>
public sealed class StatementOfWorkSlot(string code, string name, string region, string grade, string ownerId, string description, int sequence, string costCenter, double ratio, double maximum, string contactEmail, StatementOfWorkSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public string Description { get; set; } = description;
    public int Sequence { get; set; } = sequence;
    public string CostCenter { get; set; } = costCenter;
    public double Ratio { get; set; } = ratio;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public StatementOfWorkSlotStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work slot in status proposed.</summary>
    public static StatementOfWorkSlot NewStatementOfWorkSlot(string code, string name)
    {
        return new StatementOfWorkSlot(
            code,
            name,
            "",
            "",
            "",
            "",
            0,
            "",
            0.0,
            0.0,
            "",
            StatementOfWorkSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkSlotStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkSlotStatus.Proposed:
                switch (next)
                {
                    case StatementOfWorkSlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case StatementOfWorkSlotStatus.Approved:
                return next == StatementOfWorkSlotStatus.Withdrawn;
            case StatementOfWorkSlotStatus.Rejected:
                return next == StatementOfWorkSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"statement of work slot"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Sequence + Maximum, 2);
    }

    public StatementOfWorkSlot Copy()
    {
        return new StatementOfWorkSlot(
            Code,
            Name,
            Region,
            Grade,
            OwnerId,
            Description,
            Sequence,
            CostCenter,
            Ratio,
            Maximum,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
