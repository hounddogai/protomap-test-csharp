namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation window is in its life cycle.</summary>
public enum AttestationWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A attestation window. Its code is unique within a tenant.</summary>
public sealed class AttestationWindow(string code, string title, string notes, int leaveDays, double minimum, string costCenter, string description, int windowDays, bool pinned, AttestationWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int LeaveDays { get; set; } = leaveDays;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public bool Pinned { get; set; } = pinned;
    public AttestationWindowStatus Status { get; set; } = status;

    /// <summary>Starts a attestation window in status requested.</summary>
    public static AttestationWindow NewAttestationWindow(string code, string title)
    {
        return new AttestationWindow(code, title, "", 0, 0.0, "", "", 0, false, AttestationWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationWindowStatus next)
    {
        switch (Status)
        {
            case AttestationWindowStatus.Requested:
                return next == AttestationWindowStatus.Revoked || next == AttestationWindowStatus.Expired;
            case AttestationWindowStatus.Granted:
                switch (next)
                {
                    case AttestationWindowStatus.Revoked:
                        return true;
                }
                return false;
            case AttestationWindowStatus.Revoked:
                return next == AttestationWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AttestationWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"attestation window"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Minimum - (double)LeaveDays, 2);
    }

    public AttestationWindow Copy()
    {
        return new AttestationWindow(
            Code,
            Title,
            Notes,
            LeaveDays,
            Minimum,
            CostCenter,
            Description,
            WindowDays,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
