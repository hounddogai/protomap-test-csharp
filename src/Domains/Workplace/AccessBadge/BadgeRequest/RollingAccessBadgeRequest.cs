namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling access badge request is in its life cycle.</summary>
public enum RollingAccessBadgeRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One rolling access badge request, with what the planners need to act on it.</summary>
public sealed class RollingAccessBadgeRequest(string code, string label, int startsAt, string summary, string country, int endsAt, double ceiling, string timezone, double maximum, double rate, RollingAccessBadgeRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Summary { get; set; } = summary;
    public string Country { get; set; } = country;
    public int EndsAt { get; set; } = endsAt;
    public double Ceiling { get; set; } = ceiling;
    public string Timezone { get; set; } = timezone;
    public double Maximum { get; set; } = maximum;
    public double Rate { get; set; } = rate;
    public RollingAccessBadgeRequestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling access badge request in status idle.</summary>
    public static RollingAccessBadgeRequest NewRollingAccessBadgeRequest(string code, string label)
    {
        return new RollingAccessBadgeRequest(
            code,
            label,
            0,
            "",
            "",
            0,
            0.0,
            "",
            0.0,
            0.0,
            RollingAccessBadgeRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccessBadgeRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccessBadgeRequestStatus next)
    {
        switch (Status)
        {
            case RollingAccessBadgeRequestStatus.Idle:
                switch (next)
                {
                    case RollingAccessBadgeRequestStatus.Warming:
                    case RollingAccessBadgeRequestStatus.Draining:
                        return true;
                }
                return false;
            case RollingAccessBadgeRequestStatus.Warming:
                return next == RollingAccessBadgeRequestStatus.Stopped;
            case RollingAccessBadgeRequestStatus.Ready:
                return next == RollingAccessBadgeRequestStatus.Stopped;
            case RollingAccessBadgeRequestStatus.Draining:
                return next == RollingAccessBadgeRequestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingAccessBadgeRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling access badge request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ceiling * (double)StartsAt, 2);
    }

    public RollingAccessBadgeRequest Copy()
    {
        return new RollingAccessBadgeRequest(
            Code,
            Label,
            StartsAt,
            Summary,
            Country,
            EndsAt,
            Ceiling,
            Timezone,
            Maximum,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
