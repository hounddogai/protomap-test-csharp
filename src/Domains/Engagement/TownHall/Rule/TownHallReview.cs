namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall review is in its life cycle.</summary>
public enum TownHallReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A town hall review as the town hall team keeps it.</summary>
public sealed class TownHallReview(string code, string label, bool pinned, int sequence, int priority, string title, int leaveDays, string channel, int version, bool archived, double weight, TownHallReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public int Sequence { get; set; } = sequence;
    public int Priority { get; set; } = priority;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public double Weight { get; set; } = weight;
    public TownHallReviewStatus Status { get; set; } = status;

    /// <summary>Starts a town hall review in status draft.</summary>
    public static TownHallReview NewTownHallReview(string code, string label)
    {
        return new TownHallReview(code, label, false, 0, 0, "", 0, "", 0, false, 0.0, TownHallReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallReviewStatus next)
    {
        switch (Status)
        {
            case TownHallReviewStatus.Draft:
                switch (next)
                {
                    case TownHallReviewStatus.Pending:
                    case TownHallReviewStatus.Active:
                        return true;
                }
                return false;
            case TownHallReviewStatus.Pending:
                switch (next)
                {
                    case TownHallReviewStatus.Active:
                        return true;
                }
                return false;
            case TownHallReviewStatus.Active:
                return next == TownHallReviewStatus.Closed;
            case TownHallReviewStatus.Suspended:
                switch (next)
                {
                    case TownHallReviewStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"town hall review"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Priority - (double)Version, 2);
    }

    public TownHallReview Copy()
    {
        return new TownHallReview(
            Code,
            Label,
            Pinned,
            Sequence,
            Priority,
            Title,
            LeaveDays,
            Channel,
            Version,
            Archived,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
