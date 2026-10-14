namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default career path summary is in its life cycle.</summary>
public enum DefaultCareerPathSummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default career path summary as the career path team keeps it.</summary>
public sealed class DefaultCareerPathSummary(string code, string title, int endsAt, double rate, string description, int expiresAt, string label, int startsAt, string category, double ceiling, DefaultCareerPathSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public double Rate { get; set; } = rate;
    public string Description { get; set; } = description;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Category { get; set; } = category;
    public double Ceiling { get; set; } = ceiling;
    public DefaultCareerPathSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a default career path summary in status planned.</summary>
    public static DefaultCareerPathSummary NewDefaultCareerPathSummary(string code, string title)
    {
        return new DefaultCareerPathSummary(
            code,
            title,
            0,
            0.0,
            "",
            0,
            "",
            0,
            "",
            0.0,
            DefaultCareerPathSummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultCareerPathSummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultCareerPathSummaryStatus next)
    {
        switch (Status)
        {
            case DefaultCareerPathSummaryStatus.Planned:
                switch (next)
                {
                    case DefaultCareerPathSummaryStatus.Scheduled:
                        return true;
                }
                return false;
            case DefaultCareerPathSummaryStatus.Scheduled:
                switch (next)
                {
                    case DefaultCareerPathSummaryStatus.InProgress:
                        return true;
                }
                return false;
            case DefaultCareerPathSummaryStatus.InProgress:
                switch (next)
                {
                    case DefaultCareerPathSummaryStatus.Completed:
                    case DefaultCareerPathSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultCareerPathSummaryStatus.Completed:
                return next == DefaultCareerPathSummaryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultCareerPathSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default career path summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)ExpiresAt - Ceiling, 2);
    }

    public DefaultCareerPathSummary Copy()
    {
        return new DefaultCareerPathSummary(
            Code,
            Title,
            EndsAt,
            Rate,
            Description,
            ExpiresAt,
            Label,
            StartsAt,
            Category,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
