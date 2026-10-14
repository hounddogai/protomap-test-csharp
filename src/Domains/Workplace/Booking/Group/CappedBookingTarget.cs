namespace Acme.Workforce.Workplace.Booking.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped booking target is in its life cycle.</summary>
public enum CappedBookingTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped booking target, with what the planners need to act on it.</summary>
public sealed class CappedBookingTarget(string code, string name, string contactEmail, double amount, string costCenter, string segment, double ceiling, double hours, CappedBookingTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public string CostCenter { get; set; } = costCenter;
    public string Segment { get; set; } = segment;
    public double Ceiling { get; set; } = ceiling;
    public double Hours { get; set; } = hours;
    public CappedBookingTargetStatus Status { get; set; } = status;

    /// <summary>Starts a capped booking target in status proposed.</summary>
    public static CappedBookingTarget NewCappedBookingTarget(string code, string name)
    {
        return new CappedBookingTarget(code, name, "", 0.0, "", "", 0.0, 0.0, CappedBookingTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBookingTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBookingTargetStatus next)
    {
        switch (Status)
        {
            case CappedBookingTargetStatus.Proposed:
                return next == CappedBookingTargetStatus.Rejected;
            case CappedBookingTargetStatus.Approved:
                switch (next)
                {
                    case CappedBookingTargetStatus.Rejected:
                    case CappedBookingTargetStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedBookingTargetStatus.Rejected:
                return next == CappedBookingTargetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedBookingTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped booking target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round(Ceiling * Amount, 2);
    }

    public CappedBookingTarget Copy()
    {
        return new CappedBookingTarget(
            Code,
            Name,
            ContactEmail,
            Amount,
            CostCenter,
            Segment,
            Ceiling,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
