namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy span summary is in its life cycle.</summary>
public enum LegacySpanSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy span summary and its current status.</summary>
public sealed class LegacySpanSummary(string code, string name, string jobCode, List<string> tags, double rating, string ownerId, double threshold, int batchSize, int endsAt, int limit, int durationMinutes, LegacySpanSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public List<string> Tags { get; set; } = tags;
    public double Rating { get; set; } = rating;
    public string OwnerId { get; set; } = ownerId;
    public double Threshold { get; set; } = threshold;
    public int BatchSize { get; set; } = batchSize;
    public int EndsAt { get; set; } = endsAt;
    public int Limit { get; set; } = limit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LegacySpanSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy span summary in status new.</summary>
    public static LegacySpanSummary NewLegacySpanSummary(string code, string name)
    {
        return new LegacySpanSummary(
            code,
            name,
            "",
            new List<string>(),
            0.0,
            "",
            0.0,
            0,
            0,
            0,
            0,
            LegacySpanSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySpanSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySpanSummaryStatus next)
    {
        switch (Status)
        {
            case LegacySpanSummaryStatus.New:
                switch (next)
                {
                    case LegacySpanSummaryStatus.Queued:
                    case LegacySpanSummaryStatus.Done:
                        return true;
                }
                return false;
            case LegacySpanSummaryStatus.Queued:
                return next == LegacySpanSummaryStatus.Running;
            case LegacySpanSummaryStatus.Running:
                switch (next)
                {
                    case LegacySpanSummaryStatus.Done:
                    case LegacySpanSummaryStatus.Failed:
                        return true;
                }
                return false;
            case LegacySpanSummaryStatus.Done:
                switch (next)
                {
                    case LegacySpanSummaryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacySpanSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy span summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double ProjectedLoad()
    {
        return Math.Round(Threshold * (double)DurationMinutes, 2);
    }

    public LegacySpanSummary Copy()
    {
        return new LegacySpanSummary(
            Code,
            Name,
            JobCode,
            Tags,
            Rating,
            OwnerId,
            Threshold,
            BatchSize,
            EndsAt,
            Limit,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
