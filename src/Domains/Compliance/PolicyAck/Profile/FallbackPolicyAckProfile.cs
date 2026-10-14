namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy ack profile is in its life cycle.</summary>
public enum FallbackPolicyAckProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One fallback policy ack profile, with what the planners need to act on it.</summary>
public sealed class FallbackPolicyAckProfile(string code, string name, string locale, string team, int graceDays, int version, string description, bool visible, FallbackPolicyAckProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string Team { get; set; } = team;
    public int GraceDays { get; set; } = graceDays;
    public int Version { get; set; } = version;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public FallbackPolicyAckProfileStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy ack profile in status proposed.</summary>
    public static FallbackPolicyAckProfile NewFallbackPolicyAckProfile(string code, string name)
    {
        return new FallbackPolicyAckProfile(
            code,
            name,
            "",
            "",
            0,
            0,
            "",
            false,
            FallbackPolicyAckProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyAckProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyAckProfileStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyAckProfileStatus.Proposed:
                switch (next)
                {
                    case FallbackPolicyAckProfileStatus.Approved:
                    case FallbackPolicyAckProfileStatus.Rejected:
                        return true;
                }
                return false;
            case FallbackPolicyAckProfileStatus.Approved:
                return next == FallbackPolicyAckProfileStatus.Rejected;
            case FallbackPolicyAckProfileStatus.Rejected:
                return next == FallbackPolicyAckProfileStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyAckProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy ack profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"fallback policy ack profile"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Version + (double)GraceDays, 2);
    }

    public FallbackPolicyAckProfile Copy()
    {
        return new FallbackPolicyAckProfile(
            Code,
            Name,
            Locale,
            Team,
            GraceDays,
            Version,
            Description,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
