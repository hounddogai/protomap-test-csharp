namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment entry is in its life cycle.</summary>
public enum SentimentEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A sentiment entry as the sentiment team keeps it.</summary>
public sealed class SentimentEntry(string code, string name, string costCenter, int tenureMonths, bool approved, int graceDays, SentimentEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public SentimentEntryStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment entry in status draft.</summary>
    public static SentimentEntry NewSentimentEntry(string code, string name)
    {
        return new SentimentEntry(code, name, "", 0, false, 0, SentimentEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentEntryStatus next)
    {
        switch (Status)
        {
            case SentimentEntryStatus.Draft:
                return next == SentimentEntryStatus.Active;
            case SentimentEntryStatus.Pending:
                return next == SentimentEntryStatus.Active;
            case SentimentEntryStatus.Active:
                return next == SentimentEntryStatus.Suspended || next == SentimentEntryStatus.Closed;
            case SentimentEntryStatus.Suspended:
                switch (next)
                {
                    case SentimentEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round((double)GraceDays + (double)TenureMonths, 2);
    }

    public SentimentEntry Copy()
    {
        return new SentimentEntry(Code, Name, CostCenter, TenureMonths, Approved, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
