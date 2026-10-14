namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled town hall override is in its life cycle.</summary>
public enum ScheduledTownHallOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a scheduled town hall override and its current status.</summary>
public sealed class ScheduledTownHallOverride(string code, string title, string checksum, double score, string notes, double amount, double rate, string team, string reviewer, ScheduledTownHallOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public double Score { get; set; } = score;
    public string Notes { get; set; } = notes;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public string Reviewer { get; set; } = reviewer;
    public ScheduledTownHallOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled town hall override in status open.</summary>
    public static ScheduledTownHallOverride NewScheduledTownHallOverride(string code, string title)
    {
        return new ScheduledTownHallOverride(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            0.0,
            "",
            "",
            ScheduledTownHallOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTownHallOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTownHallOverrideStatus next)
    {
        switch (Status)
        {
            case ScheduledTownHallOverrideStatus.Open:
                switch (next)
                {
                    case ScheduledTownHallOverrideStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledTownHallOverrideStatus.InReview:
                switch (next)
                {
                    case ScheduledTownHallOverrideStatus.Reopened:
                    case ScheduledTownHallOverrideStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledTownHallOverrideStatus.Resolved:
                return next == ScheduledTownHallOverrideStatus.Reopened || next == ScheduledTownHallOverrideStatus.Archived;
            case ScheduledTownHallOverrideStatus.Reopened:
                switch (next)
                {
                    case ScheduledTownHallOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledTownHallOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled town hall override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Amount * Score, 2);
    }

    public ScheduledTownHallOverride Copy()
    {
        return new ScheduledTownHallOverride(
            Code,
            Title,
            Checksum,
            Score,
            Notes,
            Amount,
            Rate,
            Team,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
