namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered reorg variance is in its life cycle.</summary>
public enum TieredReorgVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One tiered reorg variance, with what the planners need to act on it.</summary>
public sealed class TieredReorgVariance(string code, string title, string segment, string timezone, List<string> tags, int updatedAt, double maximum, double score, int revision, TieredReorgVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public int Revision { get; set; } = revision;
    public TieredReorgVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a tiered reorg variance in status new.</summary>
    public static TieredReorgVariance NewTieredReorgVariance(string code, string title)
    {
        return new TieredReorgVariance(
            code,
            title,
            "",
            "",
            new List<string>(),
            0,
            0.0,
            0.0,
            0,
            TieredReorgVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredReorgVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredReorgVarianceStatus next)
    {
        switch (Status)
        {
            case TieredReorgVarianceStatus.New:
                switch (next)
                {
                    case TieredReorgVarianceStatus.Failed:
                        return true;
                }
                return false;
            case TieredReorgVarianceStatus.Queued:
                return next == TieredReorgVarianceStatus.Running || next == TieredReorgVarianceStatus.Done;
            case TieredReorgVarianceStatus.Running:
                return next == TieredReorgVarianceStatus.Done || next == TieredReorgVarianceStatus.Failed;
            case TieredReorgVarianceStatus.Done:
                switch (next)
                {
                    case TieredReorgVarianceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredReorgVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered reorg variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double AdjustedLoad()
    {
        return Math.Round(Maximum * (double)Revision, 2);
    }

    public TieredReorgVariance Copy()
    {
        return new TieredReorgVariance(
            Code,
            Title,
            Segment,
            Timezone,
            Tags,
            UpdatedAt,
            Maximum,
            Score,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
