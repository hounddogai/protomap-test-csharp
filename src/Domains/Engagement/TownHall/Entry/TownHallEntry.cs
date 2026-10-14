namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall entry is in its life cycle.</summary>
public enum TownHallEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One town hall entry, with what the planners need to act on it.</summary>
public sealed class TownHallEntry(string code, string name, string phone, int graceDays, string employeeId, int sequence, string category, TownHallEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public int GraceDays { get; set; } = graceDays;
    public string EmployeeId { get; set; } = employeeId;
    public int Sequence { get; set; } = sequence;
    public string Category { get; set; } = category;
    public TownHallEntryStatus Status { get; set; } = status;

    /// <summary>Starts a town hall entry in status new.</summary>
    public static TownHallEntry NewTownHallEntry(string code, string name)
    {
        return new TownHallEntry(code, name, "", 0, "", 0, "", TownHallEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallEntryStatus next)
    {
        switch (Status)
        {
            case TownHallEntryStatus.New:
                return next == TownHallEntryStatus.Queued || next == TownHallEntryStatus.Failed;
            case TownHallEntryStatus.Queued:
                return next == TownHallEntryStatus.Done || next == TownHallEntryStatus.Failed;
            case TownHallEntryStatus.Running:
                return next == TownHallEntryStatus.Done;
            case TownHallEntryStatus.Done:
                switch (next)
                {
                    case TownHallEntryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Sequence + (double)GraceDays, 2);
    }

    public TownHallEntry Copy()
    {
        return new TownHallEntry(Code, Name, Phone, GraceDays, EmployeeId, Sequence, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
