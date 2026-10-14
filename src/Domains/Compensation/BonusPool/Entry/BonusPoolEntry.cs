namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool entry is in its life cycle.</summary>
public enum BonusPoolEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a bonus pool entry and its current status.</summary>
public sealed class BonusPoolEntry(string code, string name, string summary, string notes, int tenureMonths, int priority, bool active, BonusPoolEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Notes { get; set; } = notes;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Priority { get; set; } = priority;
    public bool Active { get; set; } = active;
    public BonusPoolEntryStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool entry in status proposed.</summary>
    public static BonusPoolEntry NewBonusPoolEntry(string code, string name)
    {
        return new BonusPoolEntry(code, name, "", "", 0, 0, false, BonusPoolEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolEntryStatus next)
    {
        switch (Status)
        {
            case BonusPoolEntryStatus.Proposed:
                switch (next)
                {
                    case BonusPoolEntryStatus.Approved:
                    case BonusPoolEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case BonusPoolEntryStatus.Approved:
                return next == BonusPoolEntryStatus.Rejected || next == BonusPoolEntryStatus.Withdrawn;
            case BonusPoolEntryStatus.Rejected:
                switch (next)
                {
                    case BonusPoolEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths - (double)Priority, 2);
    }

    public BonusPoolEntry Copy()
    {
        return new BonusPoolEntry(Code, Name, Summary, Notes, TenureMonths, Priority, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
