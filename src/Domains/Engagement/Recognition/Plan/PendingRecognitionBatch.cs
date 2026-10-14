namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending recognition batch is in its life cycle.</summary>
public enum PendingRecognitionBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pending recognition batch, with what the planners need to act on it.</summary>
public sealed class PendingRecognitionBatch(string code, string title, double rating, string channel, double ratio, int durationMinutes, int quantity, int version, PendingRecognitionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Channel { get; set; } = channel;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public PendingRecognitionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pending recognition batch in status requested.</summary>
    public static PendingRecognitionBatch NewPendingRecognitionBatch(string code, string title)
    {
        return new PendingRecognitionBatch(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0,
            0,
            PendingRecognitionBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRecognitionBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRecognitionBatchStatus next)
    {
        switch (Status)
        {
            case PendingRecognitionBatchStatus.Requested:
                switch (next)
                {
                    case PendingRecognitionBatchStatus.Granted:
                        return true;
                }
                return false;
            case PendingRecognitionBatchStatus.Granted:
                switch (next)
                {
                    case PendingRecognitionBatchStatus.Revoked:
                        return true;
                }
                return false;
            case PendingRecognitionBatchStatus.Revoked:
                return next == PendingRecognitionBatchStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingRecognitionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending recognition batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"pending recognition batch"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Version - Ratio, 2);
    }

    public PendingRecognitionBatch Copy()
    {
        return new PendingRecognitionBatch(
            Code,
            Title,
            Rating,
            Channel,
            Ratio,
            DurationMinutes,
            Quantity,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
