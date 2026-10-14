namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday variance is in its life cycle.</summary>
public enum HolidayVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A holiday variance as the holiday team keeps it.</summary>
public sealed class HolidayVariance(string code, string name, string costCenter, string phone, double discount, string checksum, string location, bool approved, HolidayVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public string Phone { get; set; } = phone;
    public double Discount { get; set; } = discount;
    public string Checksum { get; set; } = checksum;
    public string Location { get; set; } = location;
    public bool Approved { get; set; } = approved;
    public HolidayVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a holiday variance in status planned.</summary>
    public static HolidayVariance NewHolidayVariance(string code, string name)
    {
        return new HolidayVariance(code, name, "", "", 0.0, "", "", false, HolidayVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayVarianceStatus next)
    {
        switch (Status)
        {
            case HolidayVarianceStatus.Planned:
                return next == HolidayVarianceStatus.Cancelled;
            case HolidayVarianceStatus.Scheduled:
                return next == HolidayVarianceStatus.Completed || next == HolidayVarianceStatus.Cancelled;
            case HolidayVarianceStatus.InProgress:
                return next == HolidayVarianceStatus.Cancelled;
            case HolidayVarianceStatus.Completed:
                return next == HolidayVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(HolidayVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public HolidayVariance Copy()
    {
        return new HolidayVariance(Code, Name, CostCenter, Phone, Discount, Checksum, Location, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
