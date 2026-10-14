namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient booking exception is in its life cycle.</summary>
public enum LenientBookingExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a lenient booking exception and its current status.</summary>
public sealed class LenientBookingException(string code, string name, string reviewer, string employeeId, string source, string grade, double amount, LenientBookingExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string EmployeeId { get; set; } = employeeId;
    public string Source { get; set; } = source;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public LenientBookingExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a lenient booking exception in status draft.</summary>
    public static LenientBookingException NewLenientBookingException(string code, string name)
    {
        return new LenientBookingException(code, name, "", "", "", "", 0.0, LenientBookingExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBookingExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBookingExceptionStatus next)
    {
        switch (Status)
        {
            case LenientBookingExceptionStatus.Draft:
                return next == LenientBookingExceptionStatus.Suspended;
            case LenientBookingExceptionStatus.Pending:
                return next == LenientBookingExceptionStatus.Active;
            case LenientBookingExceptionStatus.Active:
                switch (next)
                {
                    case LenientBookingExceptionStatus.Closed:
                        return true;
                }
                return false;
            case LenientBookingExceptionStatus.Suspended:
                switch (next)
                {
                    case LenientBookingExceptionStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBookingExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient booking exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient booking exception"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LenientBookingException Copy()
    {
        return new LenientBookingException(Code, Name, Reviewer, EmployeeId, Source, Grade, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
