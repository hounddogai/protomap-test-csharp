namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment group is in its life cycle.</summary>
public enum SentimentGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One sentiment group, with what the planners need to act on it.</summary>
public sealed class SentimentGroup(string code, string title, string segment, double margin, double fte, string skill, string notes, int tenureMonths, SentimentGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public double Fte { get; set; } = fte;
    public string Skill { get; set; } = skill;
    public string Notes { get; set; } = notes;
    public int TenureMonths { get; set; } = tenureMonths;
    public SentimentGroupStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment group in status requested.</summary>
    public static SentimentGroup NewSentimentGroup(string code, string title)
    {
        return new SentimentGroup(code, title, "", 0.0, 0.0, "", "", 0, SentimentGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentGroupStatus next)
    {
        switch (Status)
        {
            case SentimentGroupStatus.Requested:
                switch (next)
                {
                    case SentimentGroupStatus.Granted:
                        return true;
                }
                return false;
            case SentimentGroupStatus.Granted:
                switch (next)
                {
                    case SentimentGroupStatus.Expired:
                        return true;
                }
                return false;
            case SentimentGroupStatus.Revoked:
                switch (next)
                {
                    case SentimentGroupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment group"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Margin - (double)TenureMonths, 2);
    }

    public SentimentGroup Copy()
    {
        return new SentimentGroup(Code, Title, Segment, Margin, Fte, Skill, Notes, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
