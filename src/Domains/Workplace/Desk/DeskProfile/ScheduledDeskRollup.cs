namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled desk rollup is in its life cycle.</summary>
public enum ScheduledDeskRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled desk rollup as the desk team keeps it.</summary>
public sealed class ScheduledDeskRollup(string code, string name, double discount, double amount, double rate, string managerId, string salaryBand, ScheduledDeskRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public ScheduledDeskRollupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled desk rollup in status new.</summary>
    public static ScheduledDeskRollup NewScheduledDeskRollup(string code, string name)
    {
        return new ScheduledDeskRollup(code, name, 0.0, 0.0, 0.0, "", "", ScheduledDeskRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledDeskRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledDeskRollupStatus next)
    {
        switch (Status)
        {
            case ScheduledDeskRollupStatus.New:
                return next == ScheduledDeskRollupStatus.Running || next == ScheduledDeskRollupStatus.Failed;
            case ScheduledDeskRollupStatus.Queued:
                return next == ScheduledDeskRollupStatus.Running || next == ScheduledDeskRollupStatus.Failed;
            case ScheduledDeskRollupStatus.Running:
                return next == ScheduledDeskRollupStatus.Done || next == ScheduledDeskRollupStatus.Failed;
            case ScheduledDeskRollupStatus.Done:
                switch (next)
                {
                    case ScheduledDeskRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledDeskRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled desk rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled desk rollup"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Amount * Rate, 2);
    }

    public ScheduledDeskRollup Copy()
    {
        return new ScheduledDeskRollup(Code, Name, Discount, Amount, Rate, ManagerId, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
