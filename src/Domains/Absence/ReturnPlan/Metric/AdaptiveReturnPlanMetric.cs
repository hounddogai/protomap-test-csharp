namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive return plan metric is in its life cycle.</summary>
public enum AdaptiveReturnPlanMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adaptive return plan metric, with what the planners need to act on it.</summary>
public sealed class AdaptiveReturnPlanMetric(string code, string title, List<string> tags, string checksum, int revision, double score, int batchSize, int sequence, string team, int retries, AdaptiveReturnPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Checksum { get; set; } = checksum;
    public int Revision { get; set; } = revision;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public int Sequence { get; set; } = sequence;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public AdaptiveReturnPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive return plan metric in status open.</summary>
    public static AdaptiveReturnPlanMetric NewAdaptiveReturnPlanMetric(string code, string title)
    {
        return new AdaptiveReturnPlanMetric(
            code,
            title,
            new List<string>(),
            "",
            0,
            0.0,
            0,
            0,
            "",
            0,
            AdaptiveReturnPlanMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveReturnPlanMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveReturnPlanMetricStatus next)
    {
        switch (Status)
        {
            case AdaptiveReturnPlanMetricStatus.Open:
                switch (next)
                {
                    case AdaptiveReturnPlanMetricStatus.Resolved:
                        return true;
                }
                return false;
            case AdaptiveReturnPlanMetricStatus.InReview:
                switch (next)
                {
                    case AdaptiveReturnPlanMetricStatus.Resolved:
                    case AdaptiveReturnPlanMetricStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveReturnPlanMetricStatus.Resolved:
                return next == AdaptiveReturnPlanMetricStatus.Reopened || next == AdaptiveReturnPlanMetricStatus.Archived;
            case AdaptiveReturnPlanMetricStatus.Reopened:
                return next == AdaptiveReturnPlanMetricStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveReturnPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive return plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double EffectiveVolume()
    {
        return Math.Round((double)Revision + (double)Retries, 2);
    }

    public AdaptiveReturnPlanMetric Copy()
    {
        return new AdaptiveReturnPlanMetric(
            Code,
            Title,
            Tags,
            Checksum,
            Revision,
            Score,
            BatchSize,
            Sequence,
            Team,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
