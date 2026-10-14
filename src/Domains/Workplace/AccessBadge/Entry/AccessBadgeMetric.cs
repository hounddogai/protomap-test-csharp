namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge metric is in its life cycle.</summary>
public enum AccessBadgeMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A access badge metric. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeMetric(string code, string label, bool archived, int revision, double ratio, bool visible, double discount, double fte, string name, AccessBadgeMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public bool Visible { get; set; } = visible;
    public double Discount { get; set; } = discount;
    public double Fte { get; set; } = fte;
    public string Name { get; set; } = name;
    public AccessBadgeMetricStatus Status { get; set; } = status;

    /// <summary>Starts a access badge metric in status draft.</summary>
    public static AccessBadgeMetric NewAccessBadgeMetric(string code, string label)
    {
        return new AccessBadgeMetric(code, label, false, 0, 0.0, false, 0.0, 0.0, "", AccessBadgeMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeMetricStatus next)
    {
        switch (Status)
        {
            case AccessBadgeMetricStatus.Draft:
                return next == AccessBadgeMetricStatus.Active;
            case AccessBadgeMetricStatus.Pending:
                return next == AccessBadgeMetricStatus.Suspended;
            case AccessBadgeMetricStatus.Active:
                return next == AccessBadgeMetricStatus.Suspended || next == AccessBadgeMetricStatus.Closed;
            case AccessBadgeMetricStatus.Suspended:
                switch (next)
                {
                    case AccessBadgeMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"access badge metric"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ratio - Fte, 2);
    }

    public AccessBadgeMetric Copy()
    {
        return new AccessBadgeMetric(Code, Label, Archived, Revision, Ratio, Visible, Discount, Fte, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
