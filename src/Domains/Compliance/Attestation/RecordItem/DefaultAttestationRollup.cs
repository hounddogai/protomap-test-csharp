namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default attestation rollup is in its life cycle.</summary>
public enum DefaultAttestationRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One default attestation rollup, with what the planners need to act on it.</summary>
public sealed class DefaultAttestationRollup(string code, string label, double fte, int expiresAt, string jobCode, DefaultAttestationRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public int ExpiresAt { get; set; } = expiresAt;
    public string JobCode { get; set; } = jobCode;
    public DefaultAttestationRollupStatus Status { get; set; } = status;

    /// <summary>Starts a default attestation rollup in status proposed.</summary>
    public static DefaultAttestationRollup NewDefaultAttestationRollup(string code, string label)
    {
        return new DefaultAttestationRollup(code, label, 0.0, 0, "", DefaultAttestationRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAttestationRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAttestationRollupStatus next)
    {
        switch (Status)
        {
            case DefaultAttestationRollupStatus.Proposed:
                switch (next)
                {
                    case DefaultAttestationRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultAttestationRollupStatus.Approved:
                return next == DefaultAttestationRollupStatus.Rejected || next == DefaultAttestationRollupStatus.Withdrawn;
            case DefaultAttestationRollupStatus.Rejected:
                return next == DefaultAttestationRollupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultAttestationRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default attestation rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Fte * (double)ExpiresAt, 2);
    }

    public DefaultAttestationRollup Copy()
    {
        return new DefaultAttestationRollup(Code, Label, Fte, ExpiresAt, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
