namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement profile is in its life cycle.</summary>
public enum EngagementProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A engagement profile. Its code is unique within a tenant.</summary>
public sealed class EngagementProfile(string code, string name, string source, string ownerId, string category, double score, EngagementProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public EngagementProfileStatus Status { get; set; } = status;

    /// <summary>Starts a engagement profile in status requested.</summary>
    public static EngagementProfile NewEngagementProfile(string code, string name)
    {
        return new EngagementProfile(code, name, "", "", "", 0.0, EngagementProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementProfileStatus next)
    {
        switch (Status)
        {
            case EngagementProfileStatus.Requested:
                return next == EngagementProfileStatus.Revoked;
            case EngagementProfileStatus.Granted:
                return next == EngagementProfileStatus.Revoked || next == EngagementProfileStatus.Expired;
            case EngagementProfileStatus.Revoked:
                switch (next)
                {
                    case EngagementProfileStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public EngagementProfile Copy()
    {
        return new EngagementProfile(Code, Name, Source, OwnerId, Category, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
