namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking audit is in its life cycle.</summary>
public enum BookingAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A booking audit. Its code is unique within a tenant.</summary>
public sealed class BookingAudit(string code, string name, string phone, double rate, List<string> tags, BookingAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public BookingAuditStatus Status { get; set; } = status;

    /// <summary>Starts a booking audit in status draft.</summary>
    public static BookingAudit NewBookingAudit(string code, string name)
    {
        return new BookingAudit(code, name, "", 0.0, new List<string>(), BookingAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingAuditStatus next)
    {
        switch (Status)
        {
            case BookingAuditStatus.Draft:
                return next == BookingAuditStatus.Active;
            case BookingAuditStatus.Pending:
                return next == BookingAuditStatus.Closed;
            case BookingAuditStatus.Active:
                return next == BookingAuditStatus.Closed;
            case BookingAuditStatus.Suspended:
                return next == BookingAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BookingAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"booking audit"} {Code}";
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
        return Rate;
    }

    public BookingAudit Copy()
    {
        return new BookingAudit(Code, Name, Phone, Rate, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
