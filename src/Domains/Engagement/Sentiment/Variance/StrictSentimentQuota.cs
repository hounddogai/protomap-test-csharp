namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict sentiment quota is in its life cycle.</summary>
public enum StrictSentimentQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict sentiment quota. Its code is unique within a tenant.</summary>
public sealed class StrictSentimentQuota(string code, string label, double rate, int attempts, double threshold, bool active, string region, List<string> tags, StrictSentimentQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public double Threshold { get; set; } = threshold;
    public bool Active { get; set; } = active;
    public string Region { get; set; } = region;
    public List<string> Tags { get; set; } = tags;
    public StrictSentimentQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a strict sentiment quota in status proposed.</summary>
    public static StrictSentimentQuota NewStrictSentimentQuota(string code, string label)
    {
        return new StrictSentimentQuota(
            code,
            label,
            0.0,
            0,
            0.0,
            false,
            "",
            new List<string>(),
            StrictSentimentQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSentimentQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSentimentQuotaStatus next)
    {
        switch (Status)
        {
            case StrictSentimentQuotaStatus.Proposed:
                switch (next)
                {
                    case StrictSentimentQuotaStatus.Rejected:
                        return true;
                }
                return false;
            case StrictSentimentQuotaStatus.Approved:
                switch (next)
                {
                    case StrictSentimentQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictSentimentQuotaStatus.Rejected:
                return next == StrictSentimentQuotaStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictSentimentQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict sentiment quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round(Threshold + Rate, 2);
    }

    public StrictSentimentQuota Copy()
    {
        return new StrictSentimentQuota(Code, Label, Rate, Attempts, Threshold, Active, Region, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
