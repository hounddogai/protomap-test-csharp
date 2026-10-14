namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement bundle is in its life cycle.</summary>
public enum EngagementBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A engagement bundle. Its code is unique within a tenant.</summary>
public sealed class EngagementBundle(string code, string label, string country, string unit, double rate, EngagementBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public double Rate { get; set; } = rate;
    public EngagementBundleStatus Status { get; set; } = status;

    /// <summary>Starts a engagement bundle in status open.</summary>
    public static EngagementBundle NewEngagementBundle(string code, string label)
    {
        return new EngagementBundle(code, label, "", "", 0.0, EngagementBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementBundleStatus next)
    {
        switch (Status)
        {
            case EngagementBundleStatus.Open:
                switch (next)
                {
                    case EngagementBundleStatus.InReview:
                    case EngagementBundleStatus.Reopened:
                        return true;
                }
                return false;
            case EngagementBundleStatus.InReview:
                switch (next)
                {
                    case EngagementBundleStatus.Reopened:
                        return true;
                }
                return false;
            case EngagementBundleStatus.Resolved:
                return next == EngagementBundleStatus.Archived;
            case EngagementBundleStatus.Reopened:
                return next == EngagementBundleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(EngagementBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"engagement bundle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public EngagementBundle Copy()
    {
        return new EngagementBundle(Code, Label, Country, Unit, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
