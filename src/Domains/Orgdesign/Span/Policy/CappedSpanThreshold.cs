namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped span threshold is in its life cycle.</summary>
public enum CappedSpanThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped span threshold. Its code is unique within a tenant.</summary>
public sealed class CappedSpanThreshold(string code, string title, double budget, double cost, List<string> tags, string source, int startsAt, double weight, string skill, double maximum, int durationMinutes, CappedSpanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public string Skill { get; set; } = skill;
    public double Maximum { get; set; } = maximum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public CappedSpanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped span threshold in status draft.</summary>
    public static CappedSpanThreshold NewCappedSpanThreshold(string code, string title)
    {
        return new CappedSpanThreshold(
            code,
            title,
            0.0,
            0.0,
            new List<string>(),
            "",
            0,
            0.0,
            "",
            0.0,
            0,
            CappedSpanThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSpanThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSpanThresholdStatus next)
    {
        switch (Status)
        {
            case CappedSpanThresholdStatus.Draft:
                switch (next)
                {
                    case CappedSpanThresholdStatus.Active:
                    case CappedSpanThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case CappedSpanThresholdStatus.Pending:
                return next == CappedSpanThresholdStatus.Closed;
            case CappedSpanThresholdStatus.Active:
                return next == CappedSpanThresholdStatus.Closed;
            case CappedSpanThresholdStatus.Suspended:
                return next == CappedSpanThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedSpanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped span threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedValue()
    {
        return Math.Round(Budget - (double)DurationMinutes, 2);
    }

    public CappedSpanThreshold Copy()
    {
        return new CappedSpanThreshold(
            Code,
            Title,
            Budget,
            Cost,
            Tags,
            Source,
            StartsAt,
            Weight,
            Skill,
            Maximum,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
