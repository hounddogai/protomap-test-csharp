namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback group is in its life cycle.</summary>
public enum FeedbackGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a feedback group and its current status.</summary>
public sealed class FeedbackGroup(string code, string name, string category, int expiresAt, int capacity, string checksum, double amount, string title, int version, FeedbackGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Capacity { get; set; } = capacity;
    public string Checksum { get; set; } = checksum;
    public double Amount { get; set; } = amount;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public FeedbackGroupStatus Status { get; set; } = status;

    /// <summary>Starts a feedback group in status idle.</summary>
    public static FeedbackGroup NewFeedbackGroup(string code, string name)
    {
        return new FeedbackGroup(code, name, "", 0, 0, "", 0.0, "", 0, FeedbackGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackGroupStatus next)
    {
        switch (Status)
        {
            case FeedbackGroupStatus.Idle:
                switch (next)
                {
                    case FeedbackGroupStatus.Stopped:
                        return true;
                }
                return false;
            case FeedbackGroupStatus.Warming:
                return next == FeedbackGroupStatus.Draining;
            case FeedbackGroupStatus.Ready:
                switch (next)
                {
                    case FeedbackGroupStatus.Draining:
                    case FeedbackGroupStatus.Stopped:
                        return true;
                }
                return false;
            case FeedbackGroupStatus.Draining:
                return next == FeedbackGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FeedbackGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"feedback group"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Version - (double)ExpiresAt, 2);
    }

    public FeedbackGroup Copy()
    {
        return new FeedbackGroup(
            Code,
            Name,
            Category,
            ExpiresAt,
            Capacity,
            Checksum,
            Amount,
            Title,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
