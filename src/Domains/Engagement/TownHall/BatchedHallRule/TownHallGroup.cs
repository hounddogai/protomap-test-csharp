namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall group is in its life cycle.</summary>
public enum TownHallGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A town hall group as the town hall team keeps it.</summary>
public sealed class TownHallGroup(string code, string label, string checksum, string department, string region, int tenureMonths, double discount, double rate, int updatedAt, int sequence, TownHallGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Department { get; set; } = department;
    public string Region { get; set; } = region;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Discount { get; set; } = discount;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Sequence { get; set; } = sequence;
    public TownHallGroupStatus Status { get; set; } = status;

    /// <summary>Starts a town hall group in status requested.</summary>
    public static TownHallGroup NewTownHallGroup(string code, string label)
    {
        return new TownHallGroup(code, label, "", "", "", 0, 0.0, 0.0, 0, 0, TownHallGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallGroupStatus next)
    {
        switch (Status)
        {
            case TownHallGroupStatus.Requested:
                switch (next)
                {
                    case TownHallGroupStatus.Revoked:
                    case TownHallGroupStatus.Expired:
                        return true;
                }
                return false;
            case TownHallGroupStatus.Granted:
                switch (next)
                {
                    case TownHallGroupStatus.Revoked:
                    case TownHallGroupStatus.Expired:
                        return true;
                }
                return false;
            case TownHallGroupStatus.Revoked:
                return next == TownHallGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TownHallGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)TenureMonths + Rate, 2);
    }

    public TownHallGroup Copy()
    {
        return new TownHallGroup(
            Code,
            Label,
            Checksum,
            Department,
            Region,
            TenureMonths,
            Discount,
            Rate,
            UpdatedAt,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
