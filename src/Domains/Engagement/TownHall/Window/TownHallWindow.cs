namespace Acme.Workforce.Engagement.TownHall.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall window is in its life cycle.</summary>
public enum TownHallWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A town hall window. Its code is unique within a tenant.</summary>
public sealed class TownHallWindow(string code, string title, int endsAt, double margin, bool active, string segment, string source, int graceDays, string team, string reviewer, TownHallWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public double Margin { get; set; } = margin;
    public bool Active { get; set; } = active;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public int GraceDays { get; set; } = graceDays;
    public string Team { get; set; } = team;
    public string Reviewer { get; set; } = reviewer;
    public TownHallWindowStatus Status { get; set; } = status;

    /// <summary>Starts a town hall window in status open.</summary>
    public static TownHallWindow NewTownHallWindow(string code, string title)
    {
        return new TownHallWindow(code, title, 0, 0.0, false, "", "", 0, "", "", TownHallWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallWindowStatus next)
    {
        switch (Status)
        {
            case TownHallWindowStatus.Open:
                return next == TownHallWindowStatus.Archived;
            case TownHallWindowStatus.InReview:
                return next == TownHallWindowStatus.Reopened;
            case TownHallWindowStatus.Resolved:
                return next == TownHallWindowStatus.Archived;
            case TownHallWindowStatus.Reopened:
                return next == TownHallWindowStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TownHallWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"town hall window"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Margin * (double)EndsAt, 2);
    }

    public TownHallWindow Copy()
    {
        return new TownHallWindow(
            Code,
            Title,
            EndsAt,
            Margin,
            Active,
            Segment,
            Source,
            GraceDays,
            Team,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
