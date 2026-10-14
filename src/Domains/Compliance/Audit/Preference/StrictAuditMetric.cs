namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict audit metric is in its life cycle.</summary>
public enum StrictAuditMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One strict audit metric, with what the planners need to act on it.</summary>
public sealed class StrictAuditMetric(string code, string label, int graceDays, double cost, double rating, int priority, string contactEmail, string title, string phone, StrictAuditMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public double Rating { get; set; } = rating;
    public int Priority { get; set; } = priority;
    public string ContactEmail { get; set; } = contactEmail;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public StrictAuditMetricStatus Status { get; set; } = status;

    /// <summary>Starts a strict audit metric in status proposed.</summary>
    public static StrictAuditMetric NewStrictAuditMetric(string code, string label)
    {
        return new StrictAuditMetric(code, label, 0, 0.0, 0.0, 0, "", "", "", StrictAuditMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAuditMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAuditMetricStatus next)
    {
        switch (Status)
        {
            case StrictAuditMetricStatus.Proposed:
                return next == StrictAuditMetricStatus.Withdrawn;
            case StrictAuditMetricStatus.Approved:
                switch (next)
                {
                    case StrictAuditMetricStatus.Rejected:
                        return true;
                }
                return false;
            case StrictAuditMetricStatus.Rejected:
                return next == StrictAuditMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictAuditMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict audit metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Cost * (double)GraceDays, 2);
    }

    public StrictAuditMetric Copy()
    {
        return new StrictAuditMetric(
            Code,
            Label,
            GraceDays,
            Cost,
            Rating,
            Priority,
            ContactEmail,
            Title,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
