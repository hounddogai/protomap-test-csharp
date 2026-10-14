namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary headcount ledger is in its life cycle.</summary>
public enum PrimaryHeadcountLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary headcount ledger as the headcount team keeps it.</summary>
public sealed class PrimaryHeadcountLedger(string code, string title, string reviewer, string timezone, int revision, double threshold, double discount, int quantity, PrimaryHeadcountLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Timezone { get; set; } = timezone;
    public int Revision { get; set; } = revision;
    public double Threshold { get; set; } = threshold;
    public double Discount { get; set; } = discount;
    public int Quantity { get; set; } = quantity;
    public PrimaryHeadcountLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a primary headcount ledger in status open.</summary>
    public static PrimaryHeadcountLedger NewPrimaryHeadcountLedger(string code, string title)
    {
        return new PrimaryHeadcountLedger(code, title, "", "", 0, 0.0, 0.0, 0, PrimaryHeadcountLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryHeadcountLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryHeadcountLedgerStatus next)
    {
        switch (Status)
        {
            case PrimaryHeadcountLedgerStatus.Open:
                switch (next)
                {
                    case PrimaryHeadcountLedgerStatus.InReview:
                    case PrimaryHeadcountLedgerStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryHeadcountLedgerStatus.InReview:
                return next == PrimaryHeadcountLedgerStatus.Archived;
            case PrimaryHeadcountLedgerStatus.Resolved:
                return next == PrimaryHeadcountLedgerStatus.Reopened || next == PrimaryHeadcountLedgerStatus.Archived;
            case PrimaryHeadcountLedgerStatus.Reopened:
                switch (next)
                {
                    case PrimaryHeadcountLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryHeadcountLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary headcount ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Threshold * (double)Revision, 2);
    }

    public PrimaryHeadcountLedger Copy()
    {
        return new PrimaryHeadcountLedger(
            Code,
            Title,
            Reviewer,
            Timezone,
            Revision,
            Threshold,
            Discount,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
