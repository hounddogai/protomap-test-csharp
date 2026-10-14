namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal recognition metric is in its life cycle.</summary>
public enum SeasonalRecognitionMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a seasonal recognition metric and its current status.</summary>
public sealed class SeasonalRecognitionMetric(string code, string title, string description, int retries, double ceiling, int graceDays, SeasonalRecognitionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public int Retries { get; set; } = retries;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public SeasonalRecognitionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal recognition metric in status idle.</summary>
    public static SeasonalRecognitionMetric NewSeasonalRecognitionMetric(string code, string title)
    {
        return new SeasonalRecognitionMetric(code, title, "", 0, 0.0, 0, SeasonalRecognitionMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalRecognitionMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalRecognitionMetricStatus next)
    {
        switch (Status)
        {
            case SeasonalRecognitionMetricStatus.Idle:
                switch (next)
                {
                    case SeasonalRecognitionMetricStatus.Warming:
                    case SeasonalRecognitionMetricStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalRecognitionMetricStatus.Warming:
                switch (next)
                {
                    case SeasonalRecognitionMetricStatus.Ready:
                    case SeasonalRecognitionMetricStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalRecognitionMetricStatus.Ready:
                switch (next)
                {
                    case SeasonalRecognitionMetricStatus.Draining:
                    case SeasonalRecognitionMetricStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalRecognitionMetricStatus.Draining:
                return next == SeasonalRecognitionMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalRecognitionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal recognition metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal recognition metric"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ceiling - (double)GraceDays, 2);
    }

    public SeasonalRecognitionMetric Copy()
    {
        return new SeasonalRecognitionMetric(Code, Title, Description, Retries, Ceiling, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
