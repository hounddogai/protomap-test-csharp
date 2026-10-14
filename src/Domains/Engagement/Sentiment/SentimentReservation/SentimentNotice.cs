namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment notice is in its life cycle.</summary>
public enum SentimentNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a sentiment notice and its current status.</summary>
public sealed class SentimentNotice(string code, string label, double minimum, string channel, string badge, int tenureMonths, SentimentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Channel { get; set; } = channel;
    public string Badge { get; set; } = badge;
    public int TenureMonths { get; set; } = tenureMonths;
    public SentimentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment notice in status new.</summary>
    public static SentimentNotice NewSentimentNotice(string code, string label)
    {
        return new SentimentNotice(code, label, 0.0, "", "", 0, SentimentNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentNoticeStatus next)
    {
        switch (Status)
        {
            case SentimentNoticeStatus.New:
                return next == SentimentNoticeStatus.Failed;
            case SentimentNoticeStatus.Queued:
                return next == SentimentNoticeStatus.Done;
            case SentimentNoticeStatus.Running:
                switch (next)
                {
                    case SentimentNoticeStatus.Done:
                    case SentimentNoticeStatus.Failed:
                        return true;
                }
                return false;
            case SentimentNoticeStatus.Done:
                return next == SentimentNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SentimentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Minimum * (double)TenureMonths, 2);
    }

    public SentimentNotice Copy()
    {
        return new SentimentNotice(Code, Label, Minimum, Channel, Badge, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
