namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary warranty entry is in its life cycle.</summary>
public enum PrimaryWarrantyEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A primary warranty entry. Its code is unique within a tenant.</summary>
public sealed class PrimaryWarrantyEntry(string code, string title, string checksum, string externalRef, string segment, int quantity, double amount, string team, string channel, int revision, PrimaryWarrantyEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public int Quantity { get; set; } = quantity;
    public double Amount { get; set; } = amount;
    public string Team { get; set; } = team;
    public string Channel { get; set; } = channel;
    public int Revision { get; set; } = revision;
    public PrimaryWarrantyEntryStatus Status { get; set; } = status;

    /// <summary>Starts a primary warranty entry in status new.</summary>
    public static PrimaryWarrantyEntry NewPrimaryWarrantyEntry(string code, string title)
    {
        return new PrimaryWarrantyEntry(code, title, "", "", "", 0, 0.0, "", "", 0, PrimaryWarrantyEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryWarrantyEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryWarrantyEntryStatus next)
    {
        switch (Status)
        {
            case PrimaryWarrantyEntryStatus.New:
                switch (next)
                {
                    case PrimaryWarrantyEntryStatus.Queued:
                    case PrimaryWarrantyEntryStatus.Done:
                        return true;
                }
                return false;
            case PrimaryWarrantyEntryStatus.Queued:
                switch (next)
                {
                    case PrimaryWarrantyEntryStatus.Running:
                        return true;
                }
                return false;
            case PrimaryWarrantyEntryStatus.Running:
                return next == PrimaryWarrantyEntryStatus.Failed;
            case PrimaryWarrantyEntryStatus.Done:
                return next == PrimaryWarrantyEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryWarrantyEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary warranty entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Amount * (double)Revision, 2);
    }

    public PrimaryWarrantyEntry Copy()
    {
        return new PrimaryWarrantyEntry(
            Code,
            Title,
            Checksum,
            ExternalRef,
            Segment,
            Quantity,
            Amount,
            Team,
            Channel,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
