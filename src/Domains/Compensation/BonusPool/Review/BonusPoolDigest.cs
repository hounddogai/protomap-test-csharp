namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool digest is in its life cycle.</summary>
public enum BonusPoolDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One bonus pool digest, with what the planners need to act on it.</summary>
public sealed class BonusPoolDigest(string code, string name, double threshold, int updatedAt, int version, bool approved, BonusPoolDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Version { get; set; } = version;
    public bool Approved { get; set; } = approved;
    public BonusPoolDigestStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool digest in status draft.</summary>
    public static BonusPoolDigest NewBonusPoolDigest(string code, string name)
    {
        return new BonusPoolDigest(code, name, 0.0, 0, 0, false, BonusPoolDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolDigestStatus next)
    {
        switch (Status)
        {
            case BonusPoolDigestStatus.Draft:
                return next == BonusPoolDigestStatus.Pending || next == BonusPoolDigestStatus.Suspended;
            case BonusPoolDigestStatus.Pending:
                return next == BonusPoolDigestStatus.Active;
            case BonusPoolDigestStatus.Active:
                switch (next)
                {
                    case BonusPoolDigestStatus.Suspended:
                        return true;
                }
                return false;
            case BonusPoolDigestStatus.Suspended:
                return next == BonusPoolDigestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BonusPoolDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"bonus pool digest"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)UpdatedAt + Threshold, 2);
    }

    public BonusPoolDigest Copy()
    {
        return new BonusPoolDigest(Code, Name, Threshold, UpdatedAt, Version, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
