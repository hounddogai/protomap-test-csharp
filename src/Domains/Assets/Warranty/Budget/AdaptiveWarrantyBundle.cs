namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive warranty bundle is in its life cycle.</summary>
public enum AdaptiveWarrantyBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adaptive warranty bundle as the warranty team keeps it.</summary>
public sealed class AdaptiveWarrantyBundle(string code, string name, string badge, double fte, double margin, AdaptiveWarrantyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public double Fte { get; set; } = fte;
    public double Margin { get; set; } = margin;
    public AdaptiveWarrantyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive warranty bundle in status planned.</summary>
    public static AdaptiveWarrantyBundle NewAdaptiveWarrantyBundle(string code, string name)
    {
        return new AdaptiveWarrantyBundle(code, name, "", 0.0, 0.0, AdaptiveWarrantyBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveWarrantyBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveWarrantyBundleStatus next)
    {
        switch (Status)
        {
            case AdaptiveWarrantyBundleStatus.Planned:
                switch (next)
                {
                    case AdaptiveWarrantyBundleStatus.Scheduled:
                    case AdaptiveWarrantyBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveWarrantyBundleStatus.Scheduled:
                switch (next)
                {
                    case AdaptiveWarrantyBundleStatus.InProgress:
                    case AdaptiveWarrantyBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveWarrantyBundleStatus.InProgress:
                return next == AdaptiveWarrantyBundleStatus.Completed;
            case AdaptiveWarrantyBundleStatus.Completed:
                return next == AdaptiveWarrantyBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveWarrantyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive warranty bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Margin - Fte, 2);
    }

    public AdaptiveWarrantyBundle Copy()
    {
        return new AdaptiveWarrantyBundle(Code, Name, Badge, Fte, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
