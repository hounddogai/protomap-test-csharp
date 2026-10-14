namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday claim is in its life cycle.</summary>
public enum HolidayClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a holiday claim and its current status.</summary>
public sealed class HolidayClaim(string code, string name, string currency, string phone, double floorValue, HolidayClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string Phone { get; set; } = phone;
    public double FloorValue { get; set; } = floorValue;
    public HolidayClaimStatus Status { get; set; } = status;

    /// <summary>Starts a holiday claim in status draft.</summary>
    public static HolidayClaim NewHolidayClaim(string code, string name)
    {
        return new HolidayClaim(code, name, "", "", 0.0, HolidayClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayClaimStatus next)
    {
        switch (Status)
        {
            case HolidayClaimStatus.Draft:
                switch (next)
                {
                    case HolidayClaimStatus.Active:
                    case HolidayClaimStatus.Closed:
                        return true;
                }
                return false;
            case HolidayClaimStatus.Pending:
                return next == HolidayClaimStatus.Closed;
            case HolidayClaimStatus.Active:
                return next == HolidayClaimStatus.Suspended || next == HolidayClaimStatus.Closed;
            case HolidayClaimStatus.Suspended:
                switch (next)
                {
                    case HolidayClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public HolidayClaim Copy()
    {
        return new HolidayClaim(Code, Name, Currency, Phone, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
