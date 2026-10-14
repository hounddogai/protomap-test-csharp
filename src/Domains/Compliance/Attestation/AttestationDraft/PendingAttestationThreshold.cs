namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending attestation threshold is in its life cycle.</summary>
public enum PendingAttestationThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pending attestation threshold, with what the planners need to act on it.</summary>
public sealed class PendingAttestationThreshold(string code, string label, string department, Dictionary<string, string> attributes, string managerId, int quantity, PendingAttestationThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ManagerId { get; set; } = managerId;
    public int Quantity { get; set; } = quantity;
    public PendingAttestationThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a pending attestation threshold in status draft.</summary>
    public static PendingAttestationThreshold NewPendingAttestationThreshold(string code, string label)
    {
        return new PendingAttestationThreshold(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            PendingAttestationThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAttestationThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAttestationThresholdStatus next)
    {
        switch (Status)
        {
            case PendingAttestationThresholdStatus.Draft:
                switch (next)
                {
                    case PendingAttestationThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case PendingAttestationThresholdStatus.Pending:
                switch (next)
                {
                    case PendingAttestationThresholdStatus.Suspended:
                    case PendingAttestationThresholdStatus.Closed:
                        return true;
                }
                return false;
            case PendingAttestationThresholdStatus.Active:
                switch (next)
                {
                    case PendingAttestationThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case PendingAttestationThresholdStatus.Suspended:
                return next == PendingAttestationThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PendingAttestationThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending attestation threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending attestation threshold"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public PendingAttestationThreshold Copy()
    {
        return new PendingAttestationThreshold(Code, Label, Department, Attributes, ManagerId, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
