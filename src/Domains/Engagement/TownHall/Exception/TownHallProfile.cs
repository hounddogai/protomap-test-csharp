namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall profile is in its life cycle.</summary>
public enum TownHallProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a town hall profile and its current status.</summary>
public sealed class TownHallProfile(string code, string label, bool pinned, double score, int priority, string department, string summary, double minimum, int expiresAt, int updatedAt, TownHallProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Score { get; set; } = score;
    public int Priority { get; set; } = priority;
    public string Department { get; set; } = department;
    public string Summary { get; set; } = summary;
    public double Minimum { get; set; } = minimum;
    public int ExpiresAt { get; set; } = expiresAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public TownHallProfileStatus Status { get; set; } = status;

    /// <summary>Starts a town hall profile in status requested.</summary>
    public static TownHallProfile NewTownHallProfile(string code, string label)
    {
        return new TownHallProfile(code, label, false, 0.0, 0, "", "", 0.0, 0, 0, TownHallProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallProfileStatus next)
    {
        switch (Status)
        {
            case TownHallProfileStatus.Requested:
                return next == TownHallProfileStatus.Revoked;
            case TownHallProfileStatus.Granted:
                return next == TownHallProfileStatus.Expired;
            case TownHallProfileStatus.Revoked:
                return next == TownHallProfileStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TownHallProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)UpdatedAt - (double)Priority, 2);
    }

    public TownHallProfile Copy()
    {
        return new TownHallProfile(
            Code,
            Label,
            Pinned,
            Score,
            Priority,
            Department,
            Summary,
            Minimum,
            ExpiresAt,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
