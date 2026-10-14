namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy digest is in its life cycle.</summary>
public enum PolicyDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A policy digest as the policy team keeps it.</summary>
public sealed class PolicyDigest(string code, string name, string ownerId, string region, string title, double amount, int version, bool archived, string reviewer, PolicyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public string Region { get; set; } = region;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public PolicyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a policy digest in status open.</summary>
    public static PolicyDigest NewPolicyDigest(string code, string name)
    {
        return new PolicyDigest(code, name, "", "", "", 0.0, 0, false, "", PolicyDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyDigestStatus next)
    {
        switch (Status)
        {
            case PolicyDigestStatus.Open:
                return next == PolicyDigestStatus.Resolved || next == PolicyDigestStatus.Reopened;
            case PolicyDigestStatus.InReview:
                switch (next)
                {
                    case PolicyDigestStatus.Resolved:
                    case PolicyDigestStatus.Reopened:
                        return true;
                }
                return false;
            case PolicyDigestStatus.Resolved:
                return next == PolicyDigestStatus.Reopened || next == PolicyDigestStatus.Archived;
            case PolicyDigestStatus.Reopened:
                switch (next)
                {
                    case PolicyDigestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"policy digest"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Amount + (double)Version, 2);
    }

    public PolicyDigest Copy()
    {
        return new PolicyDigest(Code, Name, OwnerId, Region, Title, Amount, Version, Archived, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
