namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment baseline is in its life cycle.</summary>
public enum SentimentBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A sentiment baseline. Its code is unique within a tenant.</summary>
public sealed class SentimentBaseline(string code, string label, int version, int windowDays, string department, string category, string salaryBand, string timezone, string name, SentimentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public int WindowDays { get; set; } = windowDays;
    public string Department { get; set; } = department;
    public string Category { get; set; } = category;
    public string SalaryBand { get; set; } = salaryBand;
    public string Timezone { get; set; } = timezone;
    public string Name { get; set; } = name;
    public SentimentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment baseline in status proposed.</summary>
    public static SentimentBaseline NewSentimentBaseline(string code, string label)
    {
        return new SentimentBaseline(code, label, 0, 0, "", "", "", "", "", SentimentBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentBaselineStatus next)
    {
        switch (Status)
        {
            case SentimentBaselineStatus.Proposed:
                return next == SentimentBaselineStatus.Approved;
            case SentimentBaselineStatus.Approved:
                switch (next)
                {
                    case SentimentBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case SentimentBaselineStatus.Rejected:
                switch (next)
                {
                    case SentimentBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)WindowDays * (double)Version, 2);
    }

    public SentimentBaseline Copy()
    {
        return new SentimentBaseline(
            Code,
            Label,
            Version,
            WindowDays,
            Department,
            Category,
            SalaryBand,
            Timezone,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
