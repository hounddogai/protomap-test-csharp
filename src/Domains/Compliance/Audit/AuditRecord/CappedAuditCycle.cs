namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped audit cycle is in its life cycle.</summary>
public enum CappedAuditCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped audit cycle as the audit team keeps it.</summary>
public sealed class CappedAuditCycle(string code, string label, double budget, string ownerId, string location, int sequence, double discount, double fte, double weight, string category, string timezone, CappedAuditCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string OwnerId { get; set; } = ownerId;
    public string Location { get; set; } = location;
    public int Sequence { get; set; } = sequence;
    public double Discount { get; set; } = discount;
    public double Fte { get; set; } = fte;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public string Timezone { get; set; } = timezone;
    public CappedAuditCycleStatus Status { get; set; } = status;

    /// <summary>Starts a capped audit cycle in status new.</summary>
    public static CappedAuditCycle NewCappedAuditCycle(string code, string label)
    {
        return new CappedAuditCycle(code, label, 0.0, "", "", 0, 0.0, 0.0, 0.0, "", "", CappedAuditCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAuditCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAuditCycleStatus next)
    {
        switch (Status)
        {
            case CappedAuditCycleStatus.New:
                switch (next)
                {
                    case CappedAuditCycleStatus.Queued:
                    case CappedAuditCycleStatus.Running:
                        return true;
                }
                return false;
            case CappedAuditCycleStatus.Queued:
                switch (next)
                {
                    case CappedAuditCycleStatus.Running:
                    case CappedAuditCycleStatus.Failed:
                        return true;
                }
                return false;
            case CappedAuditCycleStatus.Running:
                switch (next)
                {
                    case CappedAuditCycleStatus.Done:
                        return true;
                }
                return false;
            case CappedAuditCycleStatus.Done:
                return next == CappedAuditCycleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedAuditCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped audit cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Weight - Discount, 2);
    }

    public CappedAuditCycle Copy()
    {
        return new CappedAuditCycle(
            Code,
            Label,
            Budget,
            OwnerId,
            Location,
            Sequence,
            Discount,
            Fte,
            Weight,
            Category,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
