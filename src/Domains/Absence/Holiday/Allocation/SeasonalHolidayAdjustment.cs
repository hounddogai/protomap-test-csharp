namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal holiday adjustment is in its life cycle.</summary>
public enum SeasonalHolidayAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal holiday adjustment as the holiday team keeps it.</summary>
public sealed class SeasonalHolidayAdjustment(string code, string name, string locale, double discount, string title, SeasonalHolidayAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public double Discount { get; set; } = discount;
    public string Title { get; set; } = title;
    public SeasonalHolidayAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal holiday adjustment in status idle.</summary>
    public static SeasonalHolidayAdjustment NewSeasonalHolidayAdjustment(string code, string name)
    {
        return new SeasonalHolidayAdjustment(code, name, "", 0.0, "", SeasonalHolidayAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHolidayAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHolidayAdjustmentStatus next)
    {
        switch (Status)
        {
            case SeasonalHolidayAdjustmentStatus.Idle:
                switch (next)
                {
                    case SeasonalHolidayAdjustmentStatus.Warming:
                        return true;
                }
                return false;
            case SeasonalHolidayAdjustmentStatus.Warming:
                return next == SeasonalHolidayAdjustmentStatus.Stopped;
            case SeasonalHolidayAdjustmentStatus.Ready:
                return next == SeasonalHolidayAdjustmentStatus.Draining || next == SeasonalHolidayAdjustmentStatus.Stopped;
            case SeasonalHolidayAdjustmentStatus.Draining:
                return next == SeasonalHolidayAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalHolidayAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal holiday adjustment {Code} cannot move from {Status} to {next}");
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

    public SeasonalHolidayAdjustment Copy()
    {
        return new SeasonalHolidayAdjustment(Code, Name, Locale, Discount, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
