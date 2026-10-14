namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor estimate is in its life cycle.</summary>
public enum VisitorEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One visitor estimate, with what the planners need to act on it.</summary>
public sealed class VisitorEstimate(string code, string label, int retries, string jobCode, string summary, string segment, string name, VisitorEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public string Summary { get; set; } = summary;
    public string Segment { get; set; } = segment;
    public string Name { get; set; } = name;
    public VisitorEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a visitor estimate in status new.</summary>
    public static VisitorEstimate NewVisitorEstimate(string code, string label)
    {
        return new VisitorEstimate(code, label, 0, "", "", "", "", VisitorEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorEstimateStatus next)
    {
        switch (Status)
        {
            case VisitorEstimateStatus.New:
                switch (next)
                {
                    case VisitorEstimateStatus.Running:
                        return true;
                }
                return false;
            case VisitorEstimateStatus.Queued:
                return next == VisitorEstimateStatus.Done;
            case VisitorEstimateStatus.Running:
                return next == VisitorEstimateStatus.Done;
            case VisitorEstimateStatus.Done:
                switch (next)
                {
                    case VisitorEstimateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"visitor estimate"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public VisitorEstimate Copy()
    {
        return new VisitorEstimate(Code, Label, Retries, JobCode, Summary, Segment, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
