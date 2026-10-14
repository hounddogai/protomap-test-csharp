namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary engagement notice is in its life cycle.</summary>
public enum PrimaryEngagementNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A primary engagement notice. Its code is unique within a tenant.</summary>
public sealed class PrimaryEngagementNotice(string code, string label, string source, int endsAt, bool pinned, double rating, int retries, string name, double amount, int limit, PrimaryEngagementNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public int EndsAt { get; set; } = endsAt;
    public bool Pinned { get; set; } = pinned;
    public double Rating { get; set; } = rating;
    public int Retries { get; set; } = retries;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int Limit { get; set; } = limit;
    public PrimaryEngagementNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a primary engagement notice in status new.</summary>
    public static PrimaryEngagementNotice NewPrimaryEngagementNotice(string code, string label)
    {
        return new PrimaryEngagementNotice(
            code,
            label,
            "",
            0,
            false,
            0.0,
            0,
            "",
            0.0,
            0,
            PrimaryEngagementNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryEngagementNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryEngagementNoticeStatus next)
    {
        switch (Status)
        {
            case PrimaryEngagementNoticeStatus.New:
                switch (next)
                {
                    case PrimaryEngagementNoticeStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryEngagementNoticeStatus.Queued:
                switch (next)
                {
                    case PrimaryEngagementNoticeStatus.Done:
                    case PrimaryEngagementNoticeStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryEngagementNoticeStatus.Running:
                switch (next)
                {
                    case PrimaryEngagementNoticeStatus.Done:
                    case PrimaryEngagementNoticeStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryEngagementNoticeStatus.Done:
                return next == PrimaryEngagementNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryEngagementNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary engagement notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt - Rating, 2);
    }

    public PrimaryEngagementNotice Copy()
    {
        return new PrimaryEngagementNotice(
            Code,
            Label,
            Source,
            EndsAt,
            Pinned,
            Rating,
            Retries,
            Name,
            Amount,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
