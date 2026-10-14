namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday exception is in its life cycle.</summary>
public enum HolidayExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A holiday exception. Its code is unique within a tenant.</summary>
public sealed class HolidayException(string code, string title, double rating, double fte, string ownerId, string employeeId, string unit, string costCenter, string country, HolidayExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public double Fte { get; set; } = fte;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public string Unit { get; set; } = unit;
    public string CostCenter { get; set; } = costCenter;
    public string Country { get; set; } = country;
    public HolidayExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a holiday exception in status idle.</summary>
    public static HolidayException NewHolidayException(string code, string title)
    {
        return new HolidayException(code, title, 0.0, 0.0, "", "", "", "", "", HolidayExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayExceptionStatus next)
    {
        switch (Status)
        {
            case HolidayExceptionStatus.Idle:
                switch (next)
                {
                    case HolidayExceptionStatus.Warming:
                    case HolidayExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayExceptionStatus.Warming:
                return next == HolidayExceptionStatus.Ready || next == HolidayExceptionStatus.Draining;
            case HolidayExceptionStatus.Ready:
                return next == HolidayExceptionStatus.Stopped;
            case HolidayExceptionStatus.Draining:
                switch (next)
                {
                    case HolidayExceptionStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Rating + Fte, 2);
    }

    public HolidayException Copy()
    {
        return new HolidayException(
            Code,
            Title,
            Rating,
            Fte,
            OwnerId,
            EmployeeId,
            Unit,
            CostCenter,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
