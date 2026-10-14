namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle budget is in its life cycle.</summary>
public enum MeritCycleBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a merit cycle budget and its current status.</summary>
public sealed class MeritCycleBudget(string code, string name, string department, int endsAt, string jobCode, string source, double weight, MeritCycleBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public int EndsAt { get; set; } = endsAt;
    public string JobCode { get; set; } = jobCode;
    public string Source { get; set; } = source;
    public double Weight { get; set; } = weight;
    public MeritCycleBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle budget in status proposed.</summary>
    public static MeritCycleBudget NewMeritCycleBudget(string code, string name)
    {
        return new MeritCycleBudget(code, name, "", 0, "", "", 0.0, MeritCycleBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleBudgetStatus next)
    {
        switch (Status)
        {
            case MeritCycleBudgetStatus.Proposed:
                return next == MeritCycleBudgetStatus.Rejected || next == MeritCycleBudgetStatus.Withdrawn;
            case MeritCycleBudgetStatus.Approved:
                switch (next)
                {
                    case MeritCycleBudgetStatus.Rejected:
                    case MeritCycleBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case MeritCycleBudgetStatus.Rejected:
                switch (next)
                {
                    case MeritCycleBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"merit cycle budget"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Weight * (double)EndsAt, 2);
    }

    public MeritCycleBudget Copy()
    {
        return new MeritCycleBudget(Code, Name, Department, EndsAt, JobCode, Source, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
