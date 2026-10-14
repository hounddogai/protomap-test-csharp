namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pay band tier is in its life cycle.</summary>
public enum LenientPayBandTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient pay band tier as the pay band team keeps it.</summary>
public sealed class LenientPayBandTier(string code, string title, int expiresAt, int version, string timezone, bool active, string summary, string employeeId, int retries, double cost, LenientPayBandTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Version { get; set; } = version;
    public string Timezone { get; set; } = timezone;
    public bool Active { get; set; } = active;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public LenientPayBandTierStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pay band tier in status open.</summary>
    public static LenientPayBandTier NewLenientPayBandTier(string code, string title)
    {
        return new LenientPayBandTier(code, title, 0, 0, "", false, "", "", 0, 0.0, LenientPayBandTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayBandTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayBandTierStatus next)
    {
        switch (Status)
        {
            case LenientPayBandTierStatus.Open:
                return next == LenientPayBandTierStatus.Reopened;
            case LenientPayBandTierStatus.InReview:
                return next == LenientPayBandTierStatus.Resolved || next == LenientPayBandTierStatus.Reopened;
            case LenientPayBandTierStatus.Resolved:
                return next == LenientPayBandTierStatus.Reopened;
            case LenientPayBandTierStatus.Reopened:
                return next == LenientPayBandTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientPayBandTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pay band tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Retries - (double)Version, 2);
    }

    public LenientPayBandTier Copy()
    {
        return new LenientPayBandTier(
            Code,
            Title,
            ExpiresAt,
            Version,
            Timezone,
            Active,
            Summary,
            EmployeeId,
            Retries,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
