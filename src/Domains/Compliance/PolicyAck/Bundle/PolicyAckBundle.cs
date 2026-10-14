namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack bundle is in its life cycle.</summary>
public enum PolicyAckBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a policy ack bundle and its current status.</summary>
public sealed class PolicyAckBundle(string code, string name, double minimum, int expiresAt, string locale, int revision, int durationMinutes, double fte, string description, string phone, bool active, PolicyAckBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Locale { get; set; } = locale;
    public int Revision { get; set; } = revision;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Fte { get; set; } = fte;
    public string Description { get; set; } = description;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public PolicyAckBundleStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack bundle in status proposed.</summary>
    public static PolicyAckBundle NewPolicyAckBundle(string code, string name)
    {
        return new PolicyAckBundle(code, name, 0.0, 0, "", 0, 0, 0.0, "", "", false, PolicyAckBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckBundleStatus next)
    {
        switch (Status)
        {
            case PolicyAckBundleStatus.Proposed:
                return next == PolicyAckBundleStatus.Rejected || next == PolicyAckBundleStatus.Withdrawn;
            case PolicyAckBundleStatus.Approved:
                switch (next)
                {
                    case PolicyAckBundleStatus.Rejected:
                    case PolicyAckBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyAckBundleStatus.Rejected:
                return next == PolicyAckBundleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyAckBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)DurationMinutes + (double)ExpiresAt, 2);
    }

    public PolicyAckBundle Copy()
    {
        return new PolicyAckBundle(
            Code,
            Name,
            Minimum,
            ExpiresAt,
            Locale,
            Revision,
            DurationMinutes,
            Fte,
            Description,
            Phone,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
