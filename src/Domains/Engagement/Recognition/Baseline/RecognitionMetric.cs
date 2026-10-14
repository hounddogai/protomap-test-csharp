namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition metric is in its life cycle.</summary>
public enum RecognitionMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A recognition metric. Its code is unique within a tenant.</summary>
public sealed class RecognitionMetric(string code, string name, double minimum, int leaveDays, string locale, int durationMinutes, string externalRef, int sequence, int graceDays, string badge, RecognitionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int LeaveDays { get; set; } = leaveDays;
    public string Locale { get; set; } = locale;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ExternalRef { get; set; } = externalRef;
    public int Sequence { get; set; } = sequence;
    public int GraceDays { get; set; } = graceDays;
    public string Badge { get; set; } = badge;
    public RecognitionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a recognition metric in status draft.</summary>
    public static RecognitionMetric NewRecognitionMetric(string code, string name)
    {
        return new RecognitionMetric(code, name, 0.0, 0, "", 0, "", 0, 0, "", RecognitionMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionMetricStatus next)
    {
        switch (Status)
        {
            case RecognitionMetricStatus.Draft:
                switch (next)
                {
                    case RecognitionMetricStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionMetricStatus.Pending:
                return next == RecognitionMetricStatus.Suspended;
            case RecognitionMetricStatus.Active:
                switch (next)
                {
                    case RecognitionMetricStatus.Suspended:
                    case RecognitionMetricStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionMetricStatus.Suspended:
                return next == RecognitionMetricStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RecognitionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)GraceDays * (double)DurationMinutes, 2);
    }

    public RecognitionMetric Copy()
    {
        return new RecognitionMetric(
            Code,
            Name,
            Minimum,
            LeaveDays,
            Locale,
            DurationMinutes,
            ExternalRef,
            Sequence,
            GraceDays,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
