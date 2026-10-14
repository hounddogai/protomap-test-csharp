namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global merit cycle rollup is in its life cycle.</summary>
public enum GlobalMeritCycleRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A global merit cycle rollup as the merit cycle team keeps it.</summary>
public sealed class GlobalMeritCycleRollup(string code, string name, string skill, string phone, string costCenter, Dictionary<string, string> attributes, int quantity, GlobalMeritCycleRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public string Phone { get; set; } = phone;
    public string CostCenter { get; set; } = costCenter;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Quantity { get; set; } = quantity;
    public GlobalMeritCycleRollupStatus Status { get; set; } = status;

    /// <summary>Starts a global merit cycle rollup in status new.</summary>
    public static GlobalMeritCycleRollup NewGlobalMeritCycleRollup(string code, string name)
    {
        return new GlobalMeritCycleRollup(
            code,
            name,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0,
            GlobalMeritCycleRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalMeritCycleRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalMeritCycleRollupStatus next)
    {
        switch (Status)
        {
            case GlobalMeritCycleRollupStatus.New:
                return next == GlobalMeritCycleRollupStatus.Running || next == GlobalMeritCycleRollupStatus.Failed;
            case GlobalMeritCycleRollupStatus.Queued:
                return next == GlobalMeritCycleRollupStatus.Running || next == GlobalMeritCycleRollupStatus.Done;
            case GlobalMeritCycleRollupStatus.Running:
                return next == GlobalMeritCycleRollupStatus.Done || next == GlobalMeritCycleRollupStatus.Failed;
            case GlobalMeritCycleRollupStatus.Done:
                return next == GlobalMeritCycleRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GlobalMeritCycleRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global merit cycle rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global merit cycle rollup"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public GlobalMeritCycleRollup Copy()
    {
        return new GlobalMeritCycleRollup(Code, Name, Skill, Phone, CostCenter, Attributes, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
