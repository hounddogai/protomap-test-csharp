namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor metric is in its life cycle.</summary>
public enum VisitorMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A visitor metric. Its code is unique within a tenant.</summary>
public sealed class VisitorMetric(string code, string name, string reviewer, string checksum, string locale, double score, VisitorMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string Checksum { get; set; } = checksum;
    public string Locale { get; set; } = locale;
    public double Score { get; set; } = score;
    public VisitorMetricStatus Status { get; set; } = status;

    /// <summary>Starts a visitor metric in status planned.</summary>
    public static VisitorMetric NewVisitorMetric(string code, string name)
    {
        return new VisitorMetric(code, name, "", "", "", 0.0, VisitorMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorMetricStatus next)
    {
        switch (Status)
        {
            case VisitorMetricStatus.Planned:
                return next == VisitorMetricStatus.InProgress;
            case VisitorMetricStatus.Scheduled:
                switch (next)
                {
                    case VisitorMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorMetricStatus.InProgress:
                switch (next)
                {
                    case VisitorMetricStatus.Completed:
                        return true;
                }
                return false;
            case VisitorMetricStatus.Completed:
                switch (next)
                {
                    case VisitorMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"visitor metric"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public VisitorMetric Copy()
    {
        return new VisitorMetric(Code, Name, Reviewer, Checksum, Locale, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
