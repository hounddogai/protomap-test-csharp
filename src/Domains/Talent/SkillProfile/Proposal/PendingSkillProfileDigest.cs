namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending skill profile digest is in its life cycle.</summary>
public enum PendingSkillProfileDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending skill profile digest as the skill profile team keeps it.</summary>
public sealed class PendingSkillProfileDigest(string code, string name, double threshold, string employeeId, int priority, int sequence, PendingSkillProfileDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string EmployeeId { get; set; } = employeeId;
    public int Priority { get; set; } = priority;
    public int Sequence { get; set; } = sequence;
    public PendingSkillProfileDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pending skill profile digest in status requested.</summary>
    public static PendingSkillProfileDigest NewPendingSkillProfileDigest(string code, string name)
    {
        return new PendingSkillProfileDigest(code, name, 0.0, "", 0, 0, PendingSkillProfileDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSkillProfileDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSkillProfileDigestStatus next)
    {
        switch (Status)
        {
            case PendingSkillProfileDigestStatus.Requested:
                return next == PendingSkillProfileDigestStatus.Expired;
            case PendingSkillProfileDigestStatus.Granted:
                switch (next)
                {
                    case PendingSkillProfileDigestStatus.Expired:
                        return true;
                }
                return false;
            case PendingSkillProfileDigestStatus.Revoked:
                return next == PendingSkillProfileDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingSkillProfileDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending skill profile digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Sequence - Threshold, 2);
    }

    public PendingSkillProfileDigest Copy()
    {
        return new PendingSkillProfileDigest(Code, Name, Threshold, EmployeeId, Priority, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
