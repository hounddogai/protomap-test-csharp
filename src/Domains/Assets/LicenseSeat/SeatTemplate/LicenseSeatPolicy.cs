namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat policy is in its life cycle.</summary>
public enum LicenseSeatPolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A license seat policy. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatPolicy(string code, string title, double floorValue, string timezone, int startsAt, int createdAt, bool approved, string segment, int quantity, List<string> tags, LicenseSeatPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public string Timezone { get; set; } = timezone;
    public int StartsAt { get; set; } = startsAt;
    public int CreatedAt { get; set; } = createdAt;
    public bool Approved { get; set; } = approved;
    public string Segment { get; set; } = segment;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public LicenseSeatPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a license seat policy in status requested.</summary>
    public static LicenseSeatPolicy NewLicenseSeatPolicy(string code, string title)
    {
        return new LicenseSeatPolicy(
            code,
            title,
            0.0,
            "",
            0,
            0,
            false,
            "",
            0,
            new List<string>(),
            LicenseSeatPolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatPolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatPolicyStatus next)
    {
        switch (Status)
        {
            case LicenseSeatPolicyStatus.Requested:
                switch (next)
                {
                    case LicenseSeatPolicyStatus.Granted:
                        return true;
                }
                return false;
            case LicenseSeatPolicyStatus.Granted:
                return next == LicenseSeatPolicyStatus.Expired;
            case LicenseSeatPolicyStatus.Revoked:
                switch (next)
                {
                    case LicenseSeatPolicyStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetExposure()
    {
        return Math.Round(FloorValue + (double)Quantity, 2);
    }

    public LicenseSeatPolicy Copy()
    {
        return new LicenseSeatPolicy(
            Code,
            Title,
            FloorValue,
            Timezone,
            StartsAt,
            CreatedAt,
            Approved,
            Segment,
            Quantity,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
