namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation ledger is in its life cycle.</summary>
public enum AttestationLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a attestation ledger and its current status.</summary>
public sealed class AttestationLedger(string code, string label, string channel, List<string> tags, string region, int quantity, AttestationLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public List<string> Tags { get; set; } = tags;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public AttestationLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a attestation ledger in status draft.</summary>
    public static AttestationLedger NewAttestationLedger(string code, string label)
    {
        return new AttestationLedger(code, label, "", new List<string>(), "", 0, AttestationLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationLedgerStatus next)
    {
        switch (Status)
        {
            case AttestationLedgerStatus.Draft:
                return next == AttestationLedgerStatus.Closed;
            case AttestationLedgerStatus.Pending:
                switch (next)
                {
                    case AttestationLedgerStatus.Closed:
                        return true;
                }
                return false;
            case AttestationLedgerStatus.Active:
                switch (next)
                {
                    case AttestationLedgerStatus.Closed:
                        return true;
                }
                return false;
            case AttestationLedgerStatus.Suspended:
                switch (next)
                {
                    case AttestationLedgerStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AttestationLedger Copy()
    {
        return new AttestationLedger(Code, Label, Channel, Tags, Region, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
