namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement summary is in its life cycle.</summary>
public enum EngagementSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A engagement summary. Its code is unique within a tenant.</summary>
public sealed class EngagementSummary(string code, string label, string badge, double rating, int endsAt, string name, string title, string contactEmail, string checksum, double fte, EngagementSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public int EndsAt { get; set; } = endsAt;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Checksum { get; set; } = checksum;
    public double Fte { get; set; } = fte;
    public EngagementSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a engagement summary in status new.</summary>
    public static EngagementSummary NewEngagementSummary(string code, string label)
    {
        return new EngagementSummary(code, label, "", 0.0, 0, "", "", "", "", 0.0, EngagementSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementSummaryStatus next)
    {
        switch (Status)
        {
            case EngagementSummaryStatus.New:
                switch (next)
                {
                    case EngagementSummaryStatus.Queued:
                    case EngagementSummaryStatus.Failed:
                        return true;
                }
                return false;
            case EngagementSummaryStatus.Queued:
                return next == EngagementSummaryStatus.Running || next == EngagementSummaryStatus.Done;
            case EngagementSummaryStatus.Running:
                return next == EngagementSummaryStatus.Done;
            case EngagementSummaryStatus.Done:
                return next == EngagementSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(EngagementSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt - Rating, 2);
    }

    public EngagementSummary Copy()
    {
        return new EngagementSummary(
            Code,
            Label,
            Badge,
            Rating,
            EndsAt,
            Name,
            Title,
            ContactEmail,
            Checksum,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
