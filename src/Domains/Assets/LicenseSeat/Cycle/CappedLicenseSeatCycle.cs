namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped license seat cycle is in its life cycle.</summary>
public enum CappedLicenseSeatCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a capped license seat cycle and its current status.</summary>
public sealed class CappedLicenseSeatCycle(string code, string name, int endsAt, bool pinned, string category, CappedLicenseSeatCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public bool Pinned { get; set; } = pinned;
    public string Category { get; set; } = category;
    public CappedLicenseSeatCycleStatus Status { get; set; } = status;

    /// <summary>Starts a capped license seat cycle in status new.</summary>
    public static CappedLicenseSeatCycle NewCappedLicenseSeatCycle(string code, string name)
    {
        return new CappedLicenseSeatCycle(code, name, 0, false, "", CappedLicenseSeatCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLicenseSeatCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLicenseSeatCycleStatus next)
    {
        switch (Status)
        {
            case CappedLicenseSeatCycleStatus.New:
                return next == CappedLicenseSeatCycleStatus.Running || next == CappedLicenseSeatCycleStatus.Done;
            case CappedLicenseSeatCycleStatus.Queued:
                switch (next)
                {
                    case CappedLicenseSeatCycleStatus.Running:
                        return true;
                }
                return false;
            case CappedLicenseSeatCycleStatus.Running:
                return next == CappedLicenseSeatCycleStatus.Done || next == CappedLicenseSeatCycleStatus.Failed;
            case CappedLicenseSeatCycleStatus.Done:
                return next == CappedLicenseSeatCycleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedLicenseSeatCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped license seat cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public CappedLicenseSeatCycle Copy()
    {
        return new CappedLicenseSeatCycle(Code, Name, EndsAt, Pinned, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
