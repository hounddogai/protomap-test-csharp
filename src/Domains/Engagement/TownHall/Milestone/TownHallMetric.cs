namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall metric is in its life cycle.</summary>
public enum TownHallMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a town hall metric and its current status.</summary>
public sealed class TownHallMetric(string code, string title, string location, string name, double maximum, int createdAt, int startsAt, bool active, double score, int sequence, double weight, TownHallMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int CreatedAt { get; set; } = createdAt;
    public int StartsAt { get; set; } = startsAt;
    public bool Active { get; set; } = active;
    public double Score { get; set; } = score;
    public int Sequence { get; set; } = sequence;
    public double Weight { get; set; } = weight;
    public TownHallMetricStatus Status { get; set; } = status;

    /// <summary>Starts a town hall metric in status open.</summary>
    public static TownHallMetric NewTownHallMetric(string code, string title)
    {
        return new TownHallMetric(code, title, "", "", 0.0, 0, 0, false, 0.0, 0, 0.0, TownHallMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallMetricStatus next)
    {
        switch (Status)
        {
            case TownHallMetricStatus.Open:
                switch (next)
                {
                    case TownHallMetricStatus.InReview:
                        return true;
                }
                return false;
            case TownHallMetricStatus.InReview:
                switch (next)
                {
                    case TownHallMetricStatus.Resolved:
                    case TownHallMetricStatus.Archived:
                        return true;
                }
                return false;
            case TownHallMetricStatus.Resolved:
                return next == TownHallMetricStatus.Reopened;
            case TownHallMetricStatus.Reopened:
                switch (next)
                {
                    case TownHallMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Weight - (double)StartsAt, 2);
    }

    public TownHallMetric Copy()
    {
        return new TownHallMetric(
            Code,
            Title,
            Location,
            Name,
            Maximum,
            CreatedAt,
            StartsAt,
            Active,
            Score,
            Sequence,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
