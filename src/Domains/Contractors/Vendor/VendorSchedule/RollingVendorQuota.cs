namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling vendor quota is in its life cycle.</summary>
public enum RollingVendorQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a rolling vendor quota and its current status.</summary>
public sealed class RollingVendorQuota(string code, string label, double hours, int attempts, string channel, double discount, int windowDays, double threshold, string locale, string department, string source, RollingVendorQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int Attempts { get; set; } = attempts;
    public string Channel { get; set; } = channel;
    public double Discount { get; set; } = discount;
    public int WindowDays { get; set; } = windowDays;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public string Department { get; set; } = department;
    public string Source { get; set; } = source;
    public RollingVendorQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a rolling vendor quota in status planned.</summary>
    public static RollingVendorQuota NewRollingVendorQuota(string code, string label)
    {
        return new RollingVendorQuota(
            code,
            label,
            0.0,
            0,
            "",
            0.0,
            0,
            0.0,
            "",
            "",
            "",
            RollingVendorQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingVendorQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingVendorQuotaStatus next)
    {
        switch (Status)
        {
            case RollingVendorQuotaStatus.Planned:
                switch (next)
                {
                    case RollingVendorQuotaStatus.Scheduled:
                        return true;
                }
                return false;
            case RollingVendorQuotaStatus.Scheduled:
                return next == RollingVendorQuotaStatus.Completed;
            case RollingVendorQuotaStatus.InProgress:
                return next == RollingVendorQuotaStatus.Cancelled;
            case RollingVendorQuotaStatus.Completed:
                return next == RollingVendorQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingVendorQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling vendor quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round(Discount * Hours, 2);
    }

    public RollingVendorQuota Copy()
    {
        return new RollingVendorQuota(
            Code,
            Label,
            Hours,
            Attempts,
            Channel,
            Discount,
            WindowDays,
            Threshold,
            Locale,
            Department,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
