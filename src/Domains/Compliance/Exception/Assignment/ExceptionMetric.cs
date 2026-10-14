namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception metric is in its life cycle.</summary>
public enum ExceptionMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One exception metric, with what the planners need to act on it.</summary>
public sealed class ExceptionMetric(string code, string label, int tenureMonths, int revision, string title, ExceptionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Revision { get; set; } = revision;
    public string Title { get; set; } = title;
    public ExceptionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a exception metric in status draft.</summary>
    public static ExceptionMetric NewExceptionMetric(string code, string label)
    {
        return new ExceptionMetric(code, label, 0, 0, "", ExceptionMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionMetricStatus next)
    {
        switch (Status)
        {
            case ExceptionMetricStatus.Draft:
                return next == ExceptionMetricStatus.Suspended;
            case ExceptionMetricStatus.Pending:
                return next == ExceptionMetricStatus.Closed;
            case ExceptionMetricStatus.Active:
                return next == ExceptionMetricStatus.Suspended || next == ExceptionMetricStatus.Closed;
            case ExceptionMetricStatus.Suspended:
                switch (next)
                {
                    case ExceptionMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"exception metric"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Revision - (double)TenureMonths, 2);
    }

    public ExceptionMetric Copy()
    {
        return new ExceptionMetric(Code, Label, TenureMonths, Revision, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
