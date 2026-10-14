namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy bundle is in its life cycle.</summary>
public enum AdaptivePolicyBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adaptive policy bundle as the policy team keeps it.</summary>
public sealed class AdaptivePolicyBundle(string code, string label, string locale, string summary, int windowDays, string channel, string segment, double floorValue, AdaptivePolicyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public string Channel { get; set; } = channel;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public AdaptivePolicyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy bundle in status planned.</summary>
    public static AdaptivePolicyBundle NewAdaptivePolicyBundle(string code, string label)
    {
        return new AdaptivePolicyBundle(code, label, "", "", 0, "", "", 0.0, AdaptivePolicyBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyBundleStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyBundleStatus.Planned:
                return next == AdaptivePolicyBundleStatus.Scheduled || next == AdaptivePolicyBundleStatus.Cancelled;
            case AdaptivePolicyBundleStatus.Scheduled:
                return next == AdaptivePolicyBundleStatus.Completed;
            case AdaptivePolicyBundleStatus.InProgress:
                switch (next)
                {
                    case AdaptivePolicyBundleStatus.Completed:
                        return true;
                }
                return false;
            case AdaptivePolicyBundleStatus.Completed:
                switch (next)
                {
                    case AdaptivePolicyBundleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)WindowDays - FloorValue, 2);
    }

    public AdaptivePolicyBundle Copy()
    {
        return new AdaptivePolicyBundle(
            Code,
            Label,
            Locale,
            Summary,
            WindowDays,
            Channel,
            Segment,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
