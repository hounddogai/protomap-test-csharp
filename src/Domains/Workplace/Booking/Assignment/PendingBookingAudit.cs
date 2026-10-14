namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending booking audit is in its life cycle.</summary>
public enum PendingBookingAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pending booking audit and its current status.</summary>
public sealed class PendingBookingAudit(string code, string name, int graceDays, double maximum, double cost, PendingBookingAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public PendingBookingAuditStatus Status { get; set; } = status;

    /// <summary>Starts a pending booking audit in status draft.</summary>
    public static PendingBookingAudit NewPendingBookingAudit(string code, string name)
    {
        return new PendingBookingAudit(code, name, 0, 0.0, 0.0, PendingBookingAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBookingAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBookingAuditStatus next)
    {
        switch (Status)
        {
            case PendingBookingAuditStatus.Draft:
                return next == PendingBookingAuditStatus.Pending || next == PendingBookingAuditStatus.Suspended;
            case PendingBookingAuditStatus.Pending:
                switch (next)
                {
                    case PendingBookingAuditStatus.Active:
                    case PendingBookingAuditStatus.Closed:
                        return true;
                }
                return false;
            case PendingBookingAuditStatus.Active:
                switch (next)
                {
                    case PendingBookingAuditStatus.Suspended:
                        return true;
                }
                return false;
            case PendingBookingAuditStatus.Suspended:
                return next == PendingBookingAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PendingBookingAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending booking audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Cost * Maximum, 2);
    }

    public PendingBookingAudit Copy()
    {
        return new PendingBookingAudit(Code, Name, GraceDays, Maximum, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
