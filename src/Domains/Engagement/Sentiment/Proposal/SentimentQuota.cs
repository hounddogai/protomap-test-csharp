namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment quota is in its life cycle.</summary>
public enum SentimentQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A sentiment quota. Its code is unique within a tenant.</summary>
public sealed class SentimentQuota(string code, string name, string unit, string label, int expiresAt, int quantity, int attempts, bool active, int graceDays, SentimentQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Quantity { get; set; } = quantity;
    public int Attempts { get; set; } = attempts;
    public bool Active { get; set; } = active;
    public int GraceDays { get; set; } = graceDays;
    public SentimentQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment quota in status idle.</summary>
    public static SentimentQuota NewSentimentQuota(string code, string name)
    {
        return new SentimentQuota(code, name, "", "", 0, 0, 0, false, 0, SentimentQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentQuotaStatus next)
    {
        switch (Status)
        {
            case SentimentQuotaStatus.Idle:
                return next == SentimentQuotaStatus.Warming || next == SentimentQuotaStatus.Draining;
            case SentimentQuotaStatus.Warming:
                switch (next)
                {
                    case SentimentQuotaStatus.Ready:
                    case SentimentQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case SentimentQuotaStatus.Ready:
                switch (next)
                {
                    case SentimentQuotaStatus.Draining:
                    case SentimentQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case SentimentQuotaStatus.Draining:
                return next == SentimentQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SentimentQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt * (double)GraceDays, 2);
    }

    public SentimentQuota Copy()
    {
        return new SentimentQuota(Code, Name, Unit, Label, ExpiresAt, Quantity, Attempts, Active, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
