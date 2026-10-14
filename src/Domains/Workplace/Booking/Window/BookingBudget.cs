namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking budget is in its life cycle.</summary>
public enum BookingBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A booking budget. Its code is unique within a tenant.</summary>
public sealed class BookingBudget(string code, string label, int batchSize, bool active, string description, string name, BookingBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public bool Active { get; set; } = active;
    public string Description { get; set; } = description;
    public string Name { get; set; } = name;
    public BookingBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a booking budget in status draft.</summary>
    public static BookingBudget NewBookingBudget(string code, string label)
    {
        return new BookingBudget(code, label, 0, false, "", "", BookingBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingBudgetStatus next)
    {
        switch (Status)
        {
            case BookingBudgetStatus.Draft:
                switch (next)
                {
                    case BookingBudgetStatus.Suspended:
                    case BookingBudgetStatus.Closed:
                        return true;
                }
                return false;
            case BookingBudgetStatus.Pending:
                switch (next)
                {
                    case BookingBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case BookingBudgetStatus.Active:
                return next == BookingBudgetStatus.Closed;
            case BookingBudgetStatus.Suspended:
                return next == BookingBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BookingBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public BookingBudget Copy()
    {
        return new BookingBudget(Code, Label, BatchSize, Active, Description, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
