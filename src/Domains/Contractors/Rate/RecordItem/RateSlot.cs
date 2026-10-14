namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate slot is in its life cycle.</summary>
public enum RateSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a rate slot and its current status.</summary>
public sealed class RateSlot(string code, string title, string locale, double floorValue, string skill, string jobCode, string grade, int graceDays, string department, string category, string currency, RateSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public string JobCode { get; set; } = jobCode;
    public string Grade { get; set; } = grade;
    public int GraceDays { get; set; } = graceDays;
    public string Department { get; set; } = department;
    public string Category { get; set; } = category;
    public string Currency { get; set; } = currency;
    public RateSlotStatus Status { get; set; } = status;

    /// <summary>Starts a rate slot in status planned.</summary>
    public static RateSlot NewRateSlot(string code, string title)
    {
        return new RateSlot(code, title, "", 0.0, "", "", "", 0, "", "", "", RateSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateSlotStatus next)
    {
        switch (Status)
        {
            case RateSlotStatus.Planned:
                return next == RateSlotStatus.Scheduled || next == RateSlotStatus.Completed;
            case RateSlotStatus.Scheduled:
                switch (next)
                {
                    case RateSlotStatus.InProgress:
                    case RateSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case RateSlotStatus.InProgress:
                return next == RateSlotStatus.Completed || next == RateSlotStatus.Cancelled;
            case RateSlotStatus.Completed:
                return next == RateSlotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RateSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)GraceDays + FloorValue, 2);
    }

    public RateSlot Copy()
    {
        return new RateSlot(
            Code,
            Title,
            Locale,
            FloorValue,
            Skill,
            JobCode,
            Grade,
            GraceDays,
            Department,
            Category,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
