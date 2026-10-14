namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped engagement summary is in its life cycle.</summary>
public enum CappedEngagementSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped engagement summary as the engagement team keeps it.</summary>
public sealed class CappedEngagementSummary(string code, string label, string grade, int quantity, int attempts, string name, string currency, int expiresAt, double maximum, string channel, CappedEngagementSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public int Quantity { get; set; } = quantity;
    public int Attempts { get; set; } = attempts;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Maximum { get; set; } = maximum;
    public string Channel { get; set; } = channel;
    public CappedEngagementSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a capped engagement summary in status requested.</summary>
    public static CappedEngagementSummary NewCappedEngagementSummary(string code, string label)
    {
        return new CappedEngagementSummary(
            code,
            label,
            "",
            0,
            0,
            "",
            "",
            0,
            0.0,
            "",
            CappedEngagementSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedEngagementSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedEngagementSummaryStatus next)
    {
        switch (Status)
        {
            case CappedEngagementSummaryStatus.Requested:
                switch (next)
                {
                    case CappedEngagementSummaryStatus.Revoked:
                    case CappedEngagementSummaryStatus.Expired:
                        return true;
                }
                return false;
            case CappedEngagementSummaryStatus.Granted:
                switch (next)
                {
                    case CappedEngagementSummaryStatus.Revoked:
                    case CappedEngagementSummaryStatus.Expired:
                        return true;
                }
                return false;
            case CappedEngagementSummaryStatus.Revoked:
                return next == CappedEngagementSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedEngagementSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped engagement summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped engagement summary"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Maximum * (double)Quantity, 2);
    }

    public CappedEngagementSummary Copy()
    {
        return new CappedEngagementSummary(
            Code,
            Label,
            Grade,
            Quantity,
            Attempts,
            Name,
            Currency,
            ExpiresAt,
            Maximum,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
