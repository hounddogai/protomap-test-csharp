namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default rate cycle is in its life cycle.</summary>
public enum DefaultRateCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default rate cycle and its current status.</summary>
public sealed class DefaultRateCycle(string code, string title, string location, int leaveDays, int version, int endsAt, int attempts, string region, string channel, string summary, string managerId, DefaultRateCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int LeaveDays { get; set; } = leaveDays;
    public int Version { get; set; } = version;
    public int EndsAt { get; set; } = endsAt;
    public int Attempts { get; set; } = attempts;
    public string Region { get; set; } = region;
    public string Channel { get; set; } = channel;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public DefaultRateCycleStatus Status { get; set; } = status;

    /// <summary>Starts a default rate cycle in status proposed.</summary>
    public static DefaultRateCycle NewDefaultRateCycle(string code, string title)
    {
        return new DefaultRateCycle(code, title, "", 0, 0, 0, 0, "", "", "", "", DefaultRateCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultRateCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultRateCycleStatus next)
    {
        switch (Status)
        {
            case DefaultRateCycleStatus.Proposed:
                return next == DefaultRateCycleStatus.Withdrawn;
            case DefaultRateCycleStatus.Approved:
                switch (next)
                {
                    case DefaultRateCycleStatus.Rejected:
                    case DefaultRateCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultRateCycleStatus.Rejected:
                switch (next)
                {
                    case DefaultRateCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultRateCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default rate cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)EndsAt + (double)Attempts, 2);
    }

    public DefaultRateCycle Copy()
    {
        return new DefaultRateCycle(
            Code,
            Title,
            Location,
            LeaveDays,
            Version,
            EndsAt,
            Attempts,
            Region,
            Channel,
            Summary,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
