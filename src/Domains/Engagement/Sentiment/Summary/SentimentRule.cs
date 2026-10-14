namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment rule is in its life cycle.</summary>
public enum SentimentRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a sentiment rule and its current status.</summary>
public sealed class SentimentRule(string code, string name, string channel, double threshold, string contactEmail, int leaveDays, string grade, string currency, int tenureMonths, string jobCode, SentimentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Threshold { get; set; } = threshold;
    public string ContactEmail { get; set; } = contactEmail;
    public int LeaveDays { get; set; } = leaveDays;
    public string Grade { get; set; } = grade;
    public string Currency { get; set; } = currency;
    public int TenureMonths { get; set; } = tenureMonths;
    public string JobCode { get; set; } = jobCode;
    public SentimentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment rule in status open.</summary>
    public static SentimentRule NewSentimentRule(string code, string name)
    {
        return new SentimentRule(code, name, "", 0.0, "", 0, "", "", 0, "", SentimentRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentRuleStatus next)
    {
        switch (Status)
        {
            case SentimentRuleStatus.Open:
                switch (next)
                {
                    case SentimentRuleStatus.Archived:
                        return true;
                }
                return false;
            case SentimentRuleStatus.InReview:
                return next == SentimentRuleStatus.Reopened;
            case SentimentRuleStatus.Resolved:
                switch (next)
                {
                    case SentimentRuleStatus.Archived:
                        return true;
                }
                return false;
            case SentimentRuleStatus.Reopened:
                switch (next)
                {
                    case SentimentRuleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment rule"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Threshold + (double)TenureMonths, 2);
    }

    public SentimentRule Copy()
    {
        return new SentimentRule(
            Code,
            Name,
            Channel,
            Threshold,
            ContactEmail,
            LeaveDays,
            Grade,
            Currency,
            TenureMonths,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
