namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional holiday adjustment is in its life cycle.</summary>
public enum RegionalHolidayAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional holiday adjustment. Its code is unique within a tenant.</summary>
public sealed class RegionalHolidayAdjustment(string code, string title, double margin, int quantity, string phone, string locale, RegionalHolidayAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int Quantity { get; set; } = quantity;
    public string Phone { get; set; } = phone;
    public string Locale { get; set; } = locale;
    public RegionalHolidayAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional holiday adjustment in status new.</summary>
    public static RegionalHolidayAdjustment NewRegionalHolidayAdjustment(string code, string title)
    {
        return new RegionalHolidayAdjustment(code, title, 0.0, 0, "", "", RegionalHolidayAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalHolidayAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalHolidayAdjustmentStatus next)
    {
        switch (Status)
        {
            case RegionalHolidayAdjustmentStatus.New:
                return next == RegionalHolidayAdjustmentStatus.Done || next == RegionalHolidayAdjustmentStatus.Failed;
            case RegionalHolidayAdjustmentStatus.Queued:
                switch (next)
                {
                    case RegionalHolidayAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case RegionalHolidayAdjustmentStatus.Running:
                switch (next)
                {
                    case RegionalHolidayAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case RegionalHolidayAdjustmentStatus.Done:
                switch (next)
                {
                    case RegionalHolidayAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalHolidayAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional holiday adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Quantity * Margin, 2);
    }

    public RegionalHolidayAdjustment Copy()
    {
        return new RegionalHolidayAdjustment(Code, Title, Margin, Quantity, Phone, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
