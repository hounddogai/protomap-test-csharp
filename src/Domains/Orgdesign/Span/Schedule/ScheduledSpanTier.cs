namespace Acme.Workforce.Orgdesign.Span.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled span tier is in its life cycle.</summary>
public enum ScheduledSpanTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled span tier as the span team keeps it.</summary>
public sealed class ScheduledSpanTier(string code, string title, int priority, double cost, string location, int attempts, ScheduledSpanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public int Attempts { get; set; } = attempts;
    public ScheduledSpanTierStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled span tier in status requested.</summary>
    public static ScheduledSpanTier NewScheduledSpanTier(string code, string title)
    {
        return new ScheduledSpanTier(code, title, 0, 0.0, "", 0, ScheduledSpanTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledSpanTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledSpanTierStatus next)
    {
        switch (Status)
        {
            case ScheduledSpanTierStatus.Requested:
                switch (next)
                {
                    case ScheduledSpanTierStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledSpanTierStatus.Granted:
                return next == ScheduledSpanTierStatus.Revoked;
            case ScheduledSpanTierStatus.Revoked:
                switch (next)
                {
                    case ScheduledSpanTierStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledSpanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled span tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Priority + Cost, 2);
    }

    public ScheduledSpanTier Copy()
    {
        return new ScheduledSpanTier(Code, Title, Priority, Cost, Location, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
