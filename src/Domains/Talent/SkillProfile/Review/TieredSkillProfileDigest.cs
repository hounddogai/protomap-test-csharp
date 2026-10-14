namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered skill profile digest is in its life cycle.</summary>
public enum TieredSkillProfileDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One tiered skill profile digest, with what the planners need to act on it.</summary>
public sealed class TieredSkillProfileDigest(string code, string name, double ceiling, double ratio, string costCenter, int updatedAt, double weight, int startsAt, TieredSkillProfileDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public string CostCenter { get; set; } = costCenter;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Weight { get; set; } = weight;
    public int StartsAt { get; set; } = startsAt;
    public TieredSkillProfileDigestStatus Status { get; set; } = status;

    /// <summary>Starts a tiered skill profile digest in status new.</summary>
    public static TieredSkillProfileDigest NewTieredSkillProfileDigest(string code, string name)
    {
        return new TieredSkillProfileDigest(code, name, 0.0, 0.0, "", 0, 0.0, 0, TieredSkillProfileDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSkillProfileDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSkillProfileDigestStatus next)
    {
        switch (Status)
        {
            case TieredSkillProfileDigestStatus.New:
                return next == TieredSkillProfileDigestStatus.Failed;
            case TieredSkillProfileDigestStatus.Queued:
                return next == TieredSkillProfileDigestStatus.Done || next == TieredSkillProfileDigestStatus.Failed;
            case TieredSkillProfileDigestStatus.Running:
                return next == TieredSkillProfileDigestStatus.Done;
            case TieredSkillProfileDigestStatus.Done:
                switch (next)
                {
                    case TieredSkillProfileDigestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredSkillProfileDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered skill profile digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ceiling + (double)UpdatedAt, 2);
    }

    public TieredSkillProfileDigest Copy()
    {
        return new TieredSkillProfileDigest(
            Code,
            Name,
            Ceiling,
            Ratio,
            CostCenter,
            UpdatedAt,
            Weight,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
