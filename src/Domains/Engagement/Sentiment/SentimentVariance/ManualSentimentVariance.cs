namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual sentiment variance is in its life cycle.</summary>
public enum ManualSentimentVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual sentiment variance. Its code is unique within a tenant.</summary>
public sealed class ManualSentimentVariance(string code, string label, int priority, string name, string department, ManualSentimentVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public ManualSentimentVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a manual sentiment variance in status planned.</summary>
    public static ManualSentimentVariance NewManualSentimentVariance(string code, string label)
    {
        return new ManualSentimentVariance(code, label, 0, "", "", ManualSentimentVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSentimentVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSentimentVarianceStatus next)
    {
        switch (Status)
        {
            case ManualSentimentVarianceStatus.Planned:
                return next == ManualSentimentVarianceStatus.Scheduled || next == ManualSentimentVarianceStatus.Completed;
            case ManualSentimentVarianceStatus.Scheduled:
                return next == ManualSentimentVarianceStatus.Cancelled;
            case ManualSentimentVarianceStatus.InProgress:
                switch (next)
                {
                    case ManualSentimentVarianceStatus.Completed:
                    case ManualSentimentVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualSentimentVarianceStatus.Completed:
                return next == ManualSentimentVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualSentimentVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual sentiment variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public ManualSentimentVariance Copy()
    {
        return new ManualSentimentVariance(Code, Label, Priority, Name, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
