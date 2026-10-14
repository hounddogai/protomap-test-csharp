namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment metric is in its life cycle.</summary>
public enum AssignmentMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A assignment metric. Its code is unique within a tenant.</summary>
public sealed class AssignmentMetric(string code, string label, string summary, string timezone, int leaveDays, string country, string locale, int sequence, string currency, string title, AssignmentMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public string Timezone { get; set; } = timezone;
    public int LeaveDays { get; set; } = leaveDays;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public int Sequence { get; set; } = sequence;
    public string Currency { get; set; } = currency;
    public string Title { get; set; } = title;
    public AssignmentMetricStatus Status { get; set; } = status;

    /// <summary>Starts a assignment metric in status idle.</summary>
    public static AssignmentMetric NewAssignmentMetric(string code, string label)
    {
        return new AssignmentMetric(code, label, "", "", 0, "", "", 0, "", "", AssignmentMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentMetricStatus next)
    {
        switch (Status)
        {
            case AssignmentMetricStatus.Idle:
                return next == AssignmentMetricStatus.Draining;
            case AssignmentMetricStatus.Warming:
                return next == AssignmentMetricStatus.Draining || next == AssignmentMetricStatus.Stopped;
            case AssignmentMetricStatus.Ready:
                switch (next)
                {
                    case AssignmentMetricStatus.Draining:
                    case AssignmentMetricStatus.Stopped:
                        return true;
                }
                return false;
            case AssignmentMetricStatus.Draining:
                switch (next)
                {
                    case AssignmentMetricStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)LeaveDays * (double)Sequence, 2);
    }

    public AssignmentMetric Copy()
    {
        return new AssignmentMetric(
            Code,
            Label,
            Summary,
            Timezone,
            LeaveDays,
            Country,
            Locale,
            Sequence,
            Currency,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
