namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request digest is in its life cycle.</summary>
public enum LeaveRequestDigestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One leave request digest, with what the planners need to act on it.</summary>
public sealed class LeaveRequestDigest(string code, string name, string region, string currency, string channel, double margin, double rating, bool active, int endsAt, string unit, int windowDays, LeaveRequestDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public double Margin { get; set; } = margin;
    public double Rating { get; set; } = rating;
    public bool Active { get; set; } = active;
    public int EndsAt { get; set; } = endsAt;
    public string Unit { get; set; } = unit;
    public int WindowDays { get; set; } = windowDays;
    public LeaveRequestDigestStatus Status { get; set; } = status;

    /// <summary>Starts a leave request digest in status created.</summary>
    public static LeaveRequestDigest NewLeaveRequestDigest(string code, string name)
    {
        return new LeaveRequestDigest(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0.0,
            false,
            0,
            "",
            0,
            LeaveRequestDigestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestDigestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestDigestStatus next)
    {
        switch (Status)
        {
            case LeaveRequestDigestStatus.Created:
                return next == LeaveRequestDigestStatus.Published || next == LeaveRequestDigestStatus.Retired;
            case LeaveRequestDigestStatus.Validated:
                switch (next)
                {
                    case LeaveRequestDigestStatus.Retired:
                        return true;
                }
                return false;
            case LeaveRequestDigestStatus.Published:
                return next == LeaveRequestDigestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LeaveRequestDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"leave request digest"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt - (double)WindowDays, 2);
    }

    public LeaveRequestDigest Copy()
    {
        return new LeaveRequestDigest(
            Code,
            Name,
            Region,
            Currency,
            Channel,
            Margin,
            Rating,
            Active,
            EndsAt,
            Unit,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
