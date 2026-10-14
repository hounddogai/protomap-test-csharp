namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient vendor baseline is in its life cycle.</summary>
public enum LenientVendorBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a lenient vendor baseline and its current status.</summary>
public sealed class LenientVendorBaseline(string code, string title, int version, string reviewer, int windowDays, bool visible, LenientVendorBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Reviewer { get; set; } = reviewer;
    public int WindowDays { get; set; } = windowDays;
    public bool Visible { get; set; } = visible;
    public LenientVendorBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a lenient vendor baseline in status planned.</summary>
    public static LenientVendorBaseline NewLenientVendorBaseline(string code, string title)
    {
        return new LenientVendorBaseline(code, title, 0, "", 0, false, LenientVendorBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVendorBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVendorBaselineStatus next)
    {
        switch (Status)
        {
            case LenientVendorBaselineStatus.Planned:
                switch (next)
                {
                    case LenientVendorBaselineStatus.Scheduled:
                    case LenientVendorBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientVendorBaselineStatus.Scheduled:
                switch (next)
                {
                    case LenientVendorBaselineStatus.Completed:
                    case LenientVendorBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientVendorBaselineStatus.InProgress:
                return next == LenientVendorBaselineStatus.Cancelled;
            case LenientVendorBaselineStatus.Completed:
                return next == LenientVendorBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientVendorBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient vendor baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays - (double)Version, 2);
    }

    public LenientVendorBaseline Copy()
    {
        return new LenientVendorBaseline(Code, Title, Version, Reviewer, WindowDays, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
