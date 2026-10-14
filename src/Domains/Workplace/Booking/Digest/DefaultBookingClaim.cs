namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default booking claim is in its life cycle.</summary>
public enum DefaultBookingClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A default booking claim as the booking team keeps it.</summary>
public sealed class DefaultBookingClaim(string code, string name, int expiresAt, string region, string currency, string channel, string location, int windowDays, string source, int batchSize, int startsAt, DefaultBookingClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Region { get; set; } = region;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public int WindowDays { get; set; } = windowDays;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public DefaultBookingClaimStatus Status { get; set; } = status;

    /// <summary>Starts a default booking claim in status new.</summary>
    public static DefaultBookingClaim NewDefaultBookingClaim(string code, string name)
    {
        return new DefaultBookingClaim(code, name, 0, "", "", "", "", 0, "", 0, 0, DefaultBookingClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBookingClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBookingClaimStatus next)
    {
        switch (Status)
        {
            case DefaultBookingClaimStatus.New:
                return next == DefaultBookingClaimStatus.Running;
            case DefaultBookingClaimStatus.Queued:
                return next == DefaultBookingClaimStatus.Running || next == DefaultBookingClaimStatus.Done;
            case DefaultBookingClaimStatus.Running:
                return next == DefaultBookingClaimStatus.Done || next == DefaultBookingClaimStatus.Failed;
            case DefaultBookingClaimStatus.Done:
                switch (next)
                {
                    case DefaultBookingClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultBookingClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default booking claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default booking claim"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)StartsAt + (double)BatchSize, 2);
    }

    public DefaultBookingClaim Copy()
    {
        return new DefaultBookingClaim(
            Code,
            Name,
            ExpiresAt,
            Region,
            Currency,
            Channel,
            Location,
            WindowDays,
            Source,
            BatchSize,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
