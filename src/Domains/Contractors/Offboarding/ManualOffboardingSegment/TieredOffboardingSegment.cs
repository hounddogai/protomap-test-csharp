namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered offboarding segment is in its life cycle.</summary>
public enum TieredOffboardingSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a tiered offboarding segment and its current status.</summary>
public sealed class TieredOffboardingSegment(string code, string title, string checksum, string skill, double maximum, int attempts, TieredOffboardingSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Skill { get; set; } = skill;
    public double Maximum { get; set; } = maximum;
    public int Attempts { get; set; } = attempts;
    public TieredOffboardingSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered offboarding segment in status requested.</summary>
    public static TieredOffboardingSegment NewTieredOffboardingSegment(string code, string title)
    {
        return new TieredOffboardingSegment(code, title, "", "", 0.0, 0, TieredOffboardingSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredOffboardingSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredOffboardingSegmentStatus next)
    {
        switch (Status)
        {
            case TieredOffboardingSegmentStatus.Requested:
                return next == TieredOffboardingSegmentStatus.Revoked;
            case TieredOffboardingSegmentStatus.Granted:
                return next == TieredOffboardingSegmentStatus.Revoked || next == TieredOffboardingSegmentStatus.Expired;
            case TieredOffboardingSegmentStatus.Revoked:
                switch (next)
                {
                    case TieredOffboardingSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredOffboardingSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered offboarding segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Maximum + (double)Attempts, 2);
    }

    public TieredOffboardingSegment Copy()
    {
        return new TieredOffboardingSegment(Code, Title, Checksum, Skill, Maximum, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
