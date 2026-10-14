namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle ledger is in its life cycle.</summary>
public enum MeritCycleLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A merit cycle ledger as the merit cycle team keeps it.</summary>
public sealed class MeritCycleLedger(string code, string label, string timezone, int limit, int tenureMonths, string source, string externalRef, string segment, string ownerId, MeritCycleLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public string OwnerId { get; set; } = ownerId;
    public MeritCycleLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle ledger in status proposed.</summary>
    public static MeritCycleLedger NewMeritCycleLedger(string code, string label)
    {
        return new MeritCycleLedger(code, label, "", 0, 0, "", "", "", "", MeritCycleLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleLedgerStatus next)
    {
        switch (Status)
        {
            case MeritCycleLedgerStatus.Proposed:
                switch (next)
                {
                    case MeritCycleLedgerStatus.Approved:
                        return true;
                }
                return false;
            case MeritCycleLedgerStatus.Approved:
                switch (next)
                {
                    case MeritCycleLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case MeritCycleLedgerStatus.Rejected:
                switch (next)
                {
                    case MeritCycleLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Limit * (double)TenureMonths, 2);
    }

    public MeritCycleLedger Copy()
    {
        return new MeritCycleLedger(
            Code,
            Label,
            Timezone,
            Limit,
            TenureMonths,
            Source,
            ExternalRef,
            Segment,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
