namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pay band batch is in its life cycle.</summary>
public enum FallbackPayBandBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One fallback pay band batch, with what the planners need to act on it.</summary>
public sealed class FallbackPayBandBatch(string code, string title, int attempts, string jobCode, double cost, bool active, double score, string checksum, double budget, double threshold, FallbackPayBandBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public double Cost { get; set; } = cost;
    public bool Active { get; set; } = active;
    public double Score { get; set; } = score;
    public string Checksum { get; set; } = checksum;
    public double Budget { get; set; } = budget;
    public double Threshold { get; set; } = threshold;
    public FallbackPayBandBatchStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pay band batch in status new.</summary>
    public static FallbackPayBandBatch NewFallbackPayBandBatch(string code, string title)
    {
        return new FallbackPayBandBatch(
            code,
            title,
            0,
            "",
            0.0,
            false,
            0.0,
            "",
            0.0,
            0.0,
            FallbackPayBandBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayBandBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayBandBatchStatus next)
    {
        switch (Status)
        {
            case FallbackPayBandBatchStatus.New:
                switch (next)
                {
                    case FallbackPayBandBatchStatus.Done:
                    case FallbackPayBandBatchStatus.Failed:
                        return true;
                }
                return false;
            case FallbackPayBandBatchStatus.Queued:
                switch (next)
                {
                    case FallbackPayBandBatchStatus.Done:
                    case FallbackPayBandBatchStatus.Failed:
                        return true;
                }
                return false;
            case FallbackPayBandBatchStatus.Running:
                switch (next)
                {
                    case FallbackPayBandBatchStatus.Failed:
                        return true;
                }
                return false;
            case FallbackPayBandBatchStatus.Done:
                return next == FallbackPayBandBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackPayBandBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pay band batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback pay band batch"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Score + Budget, 2);
    }

    public FallbackPayBandBatch Copy()
    {
        return new FallbackPayBandBatch(
            Code,
            Title,
            Attempts,
            JobCode,
            Cost,
            Active,
            Score,
            Checksum,
            Budget,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
