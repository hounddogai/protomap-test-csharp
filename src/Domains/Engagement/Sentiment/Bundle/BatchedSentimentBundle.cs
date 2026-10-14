namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched sentiment bundle is in its life cycle.</summary>
public enum BatchedSentimentBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched sentiment bundle as the sentiment team keeps it.</summary>
public sealed class BatchedSentimentBundle(string code, string label, double amount, string notes, double weight, string title, int startsAt, string externalRef, double score, string ownerId, BatchedSentimentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Notes { get; set; } = notes;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Score { get; set; } = score;
    public string OwnerId { get; set; } = ownerId;
    public BatchedSentimentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a batched sentiment bundle in status planned.</summary>
    public static BatchedSentimentBundle NewBatchedSentimentBundle(string code, string label)
    {
        return new BatchedSentimentBundle(
            code,
            label,
            0.0,
            "",
            0.0,
            "",
            0,
            "",
            0.0,
            "",
            BatchedSentimentBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSentimentBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSentimentBundleStatus next)
    {
        switch (Status)
        {
            case BatchedSentimentBundleStatus.Planned:
                return next == BatchedSentimentBundleStatus.Completed || next == BatchedSentimentBundleStatus.Cancelled;
            case BatchedSentimentBundleStatus.Scheduled:
                return next == BatchedSentimentBundleStatus.InProgress || next == BatchedSentimentBundleStatus.Cancelled;
            case BatchedSentimentBundleStatus.InProgress:
                switch (next)
                {
                    case BatchedSentimentBundleStatus.Completed:
                    case BatchedSentimentBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedSentimentBundleStatus.Completed:
                return next == BatchedSentimentBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedSentimentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched sentiment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)StartsAt + Weight, 2);
    }

    public BatchedSentimentBundle Copy()
    {
        return new BatchedSentimentBundle(
            Code,
            Label,
            Amount,
            Notes,
            Weight,
            Title,
            StartsAt,
            ExternalRef,
            Score,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
