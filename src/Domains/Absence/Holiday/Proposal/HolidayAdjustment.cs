namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday adjustment is in its life cycle.</summary>
public enum HolidayAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a holiday adjustment and its current status.</summary>
public sealed class HolidayAdjustment(string code, string label, double floorValue, string badge, string phone, double ratio, int version, int graceDays, HolidayAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Badge { get; set; } = badge;
    public string Phone { get; set; } = phone;
    public double Ratio { get; set; } = ratio;
    public int Version { get; set; } = version;
    public int GraceDays { get; set; } = graceDays;
    public HolidayAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a holiday adjustment in status idle.</summary>
    public static HolidayAdjustment NewHolidayAdjustment(string code, string label)
    {
        return new HolidayAdjustment(code, label, 0.0, "", "", 0.0, 0, 0, HolidayAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayAdjustmentStatus next)
    {
        switch (Status)
        {
            case HolidayAdjustmentStatus.Idle:
                return next == HolidayAdjustmentStatus.Ready;
            case HolidayAdjustmentStatus.Warming:
                switch (next)
                {
                    case HolidayAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case HolidayAdjustmentStatus.Ready:
                switch (next)
                {
                    case HolidayAdjustmentStatus.Draining:
                    case HolidayAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayAdjustmentStatus.Draining:
                return next == HolidayAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(HolidayAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version + FloorValue, 2);
    }

    public HolidayAdjustment Copy()
    {
        return new HolidayAdjustment(Code, Label, FloorValue, Badge, Phone, Ratio, Version, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
