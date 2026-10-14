namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat group is in its life cycle.</summary>
public enum LicenseSeatGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A license seat group. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatGroup(string code, string label, int createdAt, int tenureMonths, double discount, int sequence, string location, string region, int endsAt, int attempts, string badge, LicenseSeatGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Discount { get; set; } = discount;
    public int Sequence { get; set; } = sequence;
    public string Location { get; set; } = location;
    public string Region { get; set; } = region;
    public int EndsAt { get; set; } = endsAt;
    public int Attempts { get; set; } = attempts;
    public string Badge { get; set; } = badge;
    public LicenseSeatGroupStatus Status { get; set; } = status;

    /// <summary>Starts a license seat group in status requested.</summary>
    public static LicenseSeatGroup NewLicenseSeatGroup(string code, string label)
    {
        return new LicenseSeatGroup(code, label, 0, 0, 0.0, 0, "", "", 0, 0, "", LicenseSeatGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatGroupStatus next)
    {
        switch (Status)
        {
            case LicenseSeatGroupStatus.Requested:
                switch (next)
                {
                    case LicenseSeatGroupStatus.Granted:
                    case LicenseSeatGroupStatus.Revoked:
                        return true;
                }
                return false;
            case LicenseSeatGroupStatus.Granted:
                switch (next)
                {
                    case LicenseSeatGroupStatus.Revoked:
                        return true;
                }
                return false;
            case LicenseSeatGroupStatus.Revoked:
                switch (next)
                {
                    case LicenseSeatGroupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)CreatedAt * (double)TenureMonths, 2);
    }

    public LicenseSeatGroup Copy()
    {
        return new LicenseSeatGroup(
            Code,
            Label,
            CreatedAt,
            TenureMonths,
            Discount,
            Sequence,
            Location,
            Region,
            EndsAt,
            Attempts,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
