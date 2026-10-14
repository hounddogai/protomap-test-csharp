namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global attestation baseline is in its life cycle.</summary>
public enum GlobalAttestationBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One global attestation baseline, with what the planners need to act on it.</summary>
public sealed class GlobalAttestationBaseline(string code, string label, double hours, int quantity, int version, string managerId, int expiresAt, GlobalAttestationBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public GlobalAttestationBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a global attestation baseline in status proposed.</summary>
    public static GlobalAttestationBaseline NewGlobalAttestationBaseline(string code, string label)
    {
        return new GlobalAttestationBaseline(
            code,
            label,
            0.0,
            0,
            0,
            "",
            0,
            GlobalAttestationBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAttestationBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAttestationBaselineStatus next)
    {
        switch (Status)
        {
            case GlobalAttestationBaselineStatus.Proposed:
                return next == GlobalAttestationBaselineStatus.Approved || next == GlobalAttestationBaselineStatus.Withdrawn;
            case GlobalAttestationBaselineStatus.Approved:
                switch (next)
                {
                    case GlobalAttestationBaselineStatus.Rejected:
                    case GlobalAttestationBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case GlobalAttestationBaselineStatus.Rejected:
                switch (next)
                {
                    case GlobalAttestationBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAttestationBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global attestation baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"global attestation baseline"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Hours - (double)ExpiresAt, 2);
    }

    public GlobalAttestationBaseline Copy()
    {
        return new GlobalAttestationBaseline(Code, Label, Hours, Quantity, Version, ManagerId, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
