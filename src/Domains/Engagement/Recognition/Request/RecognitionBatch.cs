namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition batch is in its life cycle.</summary>
public enum RecognitionBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A recognition batch as the recognition team keeps it.</summary>
public sealed class RecognitionBatch(string code, string label, double threshold, bool pinned, double hours, string source, string timezone, string reviewer, double maximum, RecognitionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public string Source { get; set; } = source;
    public string Timezone { get; set; } = timezone;
    public string Reviewer { get; set; } = reviewer;
    public double Maximum { get; set; } = maximum;
    public RecognitionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a recognition batch in status planned.</summary>
    public static RecognitionBatch NewRecognitionBatch(string code, string label)
    {
        return new RecognitionBatch(code, label, 0.0, false, 0.0, "", "", "", 0.0, RecognitionBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionBatchStatus next)
    {
        switch (Status)
        {
            case RecognitionBatchStatus.Planned:
                switch (next)
                {
                    case RecognitionBatchStatus.Scheduled:
                        return true;
                }
                return false;
            case RecognitionBatchStatus.Scheduled:
                switch (next)
                {
                    case RecognitionBatchStatus.InProgress:
                    case RecognitionBatchStatus.Completed:
                        return true;
                }
                return false;
            case RecognitionBatchStatus.InProgress:
                return next == RecognitionBatchStatus.Completed || next == RecognitionBatchStatus.Cancelled;
            case RecognitionBatchStatus.Completed:
                switch (next)
                {
                    case RecognitionBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Threshold + Hours, 2);
    }

    public RecognitionBatch Copy()
    {
        return new RecognitionBatch(
            Code,
            Label,
            Threshold,
            Pinned,
            Hours,
            Source,
            Timezone,
            Reviewer,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
