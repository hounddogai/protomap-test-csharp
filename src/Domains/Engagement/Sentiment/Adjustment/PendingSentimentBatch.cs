namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending sentiment batch is in its life cycle.</summary>
public enum PendingSentimentBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pending sentiment batch, with what the planners need to act on it.</summary>
public sealed class PendingSentimentBatch(string code, string name, Dictionary<string, string> attributes, bool archived, string ownerId, int quantity, string reviewer, PendingSentimentBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Archived { get; set; } = archived;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public string Reviewer { get; set; } = reviewer;
    public PendingSentimentBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pending sentiment batch in status new.</summary>
    public static PendingSentimentBatch NewPendingSentimentBatch(string code, string name)
    {
        return new PendingSentimentBatch(
            code,
            name,
            new Dictionary<string, string>(),
            false,
            "",
            0,
            "",
            PendingSentimentBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSentimentBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSentimentBatchStatus next)
    {
        switch (Status)
        {
            case PendingSentimentBatchStatus.New:
                switch (next)
                {
                    case PendingSentimentBatchStatus.Queued:
                    case PendingSentimentBatchStatus.Running:
                        return true;
                }
                return false;
            case PendingSentimentBatchStatus.Queued:
                switch (next)
                {
                    case PendingSentimentBatchStatus.Done:
                        return true;
                }
                return false;
            case PendingSentimentBatchStatus.Running:
                return next == PendingSentimentBatchStatus.Failed;
            case PendingSentimentBatchStatus.Done:
                return next == PendingSentimentBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingSentimentBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending sentiment batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public PendingSentimentBatch Copy()
    {
        return new PendingSentimentBatch(Code, Name, Attributes, Archived, OwnerId, Quantity, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
