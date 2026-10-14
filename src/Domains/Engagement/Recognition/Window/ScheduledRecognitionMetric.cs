namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled recognition metric is in its life cycle.</summary>
public enum ScheduledRecognitionMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled recognition metric. Its code is unique within a tenant.</summary>
public sealed class ScheduledRecognitionMetric(string code, string name, int batchSize, double threshold, bool active, ScheduledRecognitionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public double Threshold { get; set; } = threshold;
    public bool Active { get; set; } = active;
    public ScheduledRecognitionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled recognition metric in status draft.</summary>
    public static ScheduledRecognitionMetric NewScheduledRecognitionMetric(string code, string name)
    {
        return new ScheduledRecognitionMetric(code, name, 0, 0.0, false, ScheduledRecognitionMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRecognitionMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRecognitionMetricStatus next)
    {
        switch (Status)
        {
            case ScheduledRecognitionMetricStatus.Draft:
                return next == ScheduledRecognitionMetricStatus.Suspended;
            case ScheduledRecognitionMetricStatus.Pending:
                switch (next)
                {
                    case ScheduledRecognitionMetricStatus.Active:
                        return true;
                }
                return false;
            case ScheduledRecognitionMetricStatus.Active:
                switch (next)
                {
                    case ScheduledRecognitionMetricStatus.Suspended:
                    case ScheduledRecognitionMetricStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledRecognitionMetricStatus.Suspended:
                return next == ScheduledRecognitionMetricStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledRecognitionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled recognition metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize - Threshold, 2);
    }

    public ScheduledRecognitionMetric Copy()
    {
        return new ScheduledRecognitionMetric(Code, Name, BatchSize, Threshold, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
