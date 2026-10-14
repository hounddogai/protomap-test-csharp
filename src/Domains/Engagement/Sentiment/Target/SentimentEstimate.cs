namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment estimate is in its life cycle.</summary>
public enum SentimentEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A sentiment estimate. Its code is unique within a tenant.</summary>
public sealed class SentimentEstimate(string code, string label, int retries, double rate, int tenureMonths, string channel, string department, SentimentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Channel { get; set; } = channel;
    public string Department { get; set; } = department;
    public SentimentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment estimate in status requested.</summary>
    public static SentimentEstimate NewSentimentEstimate(string code, string label)
    {
        return new SentimentEstimate(code, label, 0, 0.0, 0, "", "", SentimentEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentEstimateStatus next)
    {
        switch (Status)
        {
            case SentimentEstimateStatus.Requested:
                switch (next)
                {
                    case SentimentEstimateStatus.Granted:
                    case SentimentEstimateStatus.Revoked:
                        return true;
                }
                return false;
            case SentimentEstimateStatus.Granted:
                switch (next)
                {
                    case SentimentEstimateStatus.Revoked:
                    case SentimentEstimateStatus.Expired:
                        return true;
                }
                return false;
            case SentimentEstimateStatus.Revoked:
                switch (next)
                {
                    case SentimentEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)TenureMonths - Rate, 2);
    }

    public SentimentEstimate Copy()
    {
        return new SentimentEstimate(Code, Label, Retries, Rate, TenureMonths, Channel, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
