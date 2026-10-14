namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling access badge tier is in its life cycle.</summary>
public enum RollingAccessBadgeTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One rolling access badge tier, with what the planners need to act on it.</summary>
public sealed class RollingAccessBadgeTier(string code, string name, int updatedAt, string description, int tenureMonths, string contactEmail, int startsAt, bool approved, double threshold, string locale, string checksum, RollingAccessBadgeTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Description { get; set; } = description;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ContactEmail { get; set; } = contactEmail;
    public int StartsAt { get; set; } = startsAt;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public string Checksum { get; set; } = checksum;
    public RollingAccessBadgeTierStatus Status { get; set; } = status;

    /// <summary>Starts a rolling access badge tier in status proposed.</summary>
    public static RollingAccessBadgeTier NewRollingAccessBadgeTier(string code, string name)
    {
        return new RollingAccessBadgeTier(
            code,
            name,
            0,
            "",
            0,
            "",
            0,
            false,
            0.0,
            "",
            "",
            RollingAccessBadgeTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccessBadgeTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccessBadgeTierStatus next)
    {
        switch (Status)
        {
            case RollingAccessBadgeTierStatus.Proposed:
                switch (next)
                {
                    case RollingAccessBadgeTierStatus.Rejected:
                        return true;
                }
                return false;
            case RollingAccessBadgeTierStatus.Approved:
                return next == RollingAccessBadgeTierStatus.Rejected;
            case RollingAccessBadgeTierStatus.Rejected:
                switch (next)
                {
                    case RollingAccessBadgeTierStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAccessBadgeTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling access badge tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)StartsAt + Threshold, 2);
    }

    public RollingAccessBadgeTier Copy()
    {
        return new RollingAccessBadgeTier(
            Code,
            Name,
            UpdatedAt,
            Description,
            TenureMonths,
            ContactEmail,
            StartsAt,
            Approved,
            Threshold,
            Locale,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
