namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default attestation claim is in its life cycle.</summary>
public enum DefaultAttestationClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default attestation claim and its current status.</summary>
public sealed class DefaultAttestationClaim(string code, string name, string summary, int windowDays, double rate, string timezone, string costCenter, double threshold, int priority, int startsAt, DefaultAttestationClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public double Rate { get; set; } = rate;
    public string Timezone { get; set; } = timezone;
    public string CostCenter { get; set; } = costCenter;
    public double Threshold { get; set; } = threshold;
    public int Priority { get; set; } = priority;
    public int StartsAt { get; set; } = startsAt;
    public DefaultAttestationClaimStatus Status { get; set; } = status;

    /// <summary>Starts a default attestation claim in status proposed.</summary>
    public static DefaultAttestationClaim NewDefaultAttestationClaim(string code, string name)
    {
        return new DefaultAttestationClaim(
            code,
            name,
            "",
            0,
            0.0,
            "",
            "",
            0.0,
            0,
            0,
            DefaultAttestationClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAttestationClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAttestationClaimStatus next)
    {
        switch (Status)
        {
            case DefaultAttestationClaimStatus.Proposed:
                switch (next)
                {
                    case DefaultAttestationClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultAttestationClaimStatus.Approved:
                return next == DefaultAttestationClaimStatus.Rejected || next == DefaultAttestationClaimStatus.Withdrawn;
            case DefaultAttestationClaimStatus.Rejected:
                switch (next)
                {
                    case DefaultAttestationClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultAttestationClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default attestation claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default attestation claim"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate * (double)Priority, 2);
    }

    public DefaultAttestationClaim Copy()
    {
        return new DefaultAttestationClaim(
            Code,
            Name,
            Summary,
            WindowDays,
            Rate,
            Timezone,
            CostCenter,
            Threshold,
            Priority,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
