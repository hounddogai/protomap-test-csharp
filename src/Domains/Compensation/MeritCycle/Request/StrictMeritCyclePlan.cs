namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict merit cycle plan is in its life cycle.</summary>
public enum StrictMeritCyclePlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a strict merit cycle plan and its current status.</summary>
public sealed class StrictMeritCyclePlan(string code, string name, int windowDays, int startsAt, string location, string currency, string description, string channel, StrictMeritCyclePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public StrictMeritCyclePlanStatus Status { get; set; } = status;

    /// <summary>Starts a strict merit cycle plan in status open.</summary>
    public static StrictMeritCyclePlan NewStrictMeritCyclePlan(string code, string name)
    {
        return new StrictMeritCyclePlan(code, name, 0, 0, "", "", "", "", StrictMeritCyclePlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictMeritCyclePlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictMeritCyclePlanStatus next)
    {
        switch (Status)
        {
            case StrictMeritCyclePlanStatus.Open:
                switch (next)
                {
                    case StrictMeritCyclePlanStatus.InReview:
                    case StrictMeritCyclePlanStatus.Archived:
                        return true;
                }
                return false;
            case StrictMeritCyclePlanStatus.InReview:
                switch (next)
                {
                    case StrictMeritCyclePlanStatus.Reopened:
                        return true;
                }
                return false;
            case StrictMeritCyclePlanStatus.Resolved:
                switch (next)
                {
                    case StrictMeritCyclePlanStatus.Reopened:
                    case StrictMeritCyclePlanStatus.Archived:
                        return true;
                }
                return false;
            case StrictMeritCyclePlanStatus.Reopened:
                return next == StrictMeritCyclePlanStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictMeritCyclePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict merit cycle plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)StartsAt - (double)WindowDays, 2);
    }

    public StrictMeritCyclePlan Copy()
    {
        return new StrictMeritCyclePlan(
            Code,
            Name,
            WindowDays,
            StartsAt,
            Location,
            Currency,
            Description,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
