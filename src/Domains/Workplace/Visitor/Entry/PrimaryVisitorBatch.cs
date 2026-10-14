namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary visitor batch is in its life cycle.</summary>
public enum PrimaryVisitorBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary visitor batch as the visitor team keeps it.</summary>
public sealed class PrimaryVisitorBatch(string code, string label, string contactEmail, int limit, int sequence, PrimaryVisitorBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public int Limit { get; set; } = limit;
    public int Sequence { get; set; } = sequence;
    public PrimaryVisitorBatchStatus Status { get; set; } = status;

    /// <summary>Starts a primary visitor batch in status requested.</summary>
    public static PrimaryVisitorBatch NewPrimaryVisitorBatch(string code, string label)
    {
        return new PrimaryVisitorBatch(code, label, "", 0, 0, PrimaryVisitorBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryVisitorBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryVisitorBatchStatus next)
    {
        switch (Status)
        {
            case PrimaryVisitorBatchStatus.Requested:
                return next == PrimaryVisitorBatchStatus.Expired;
            case PrimaryVisitorBatchStatus.Granted:
                return next == PrimaryVisitorBatchStatus.Expired;
            case PrimaryVisitorBatchStatus.Revoked:
                return next == PrimaryVisitorBatchStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PrimaryVisitorBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary visitor batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Sequence + (double)Limit, 2);
    }

    public PrimaryVisitorBatch Copy()
    {
        return new PrimaryVisitorBatch(Code, Label, ContactEmail, Limit, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
