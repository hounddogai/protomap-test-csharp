namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle plan is in its life cycle.</summary>
public enum MeritCyclePlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A merit cycle plan. Its code is unique within a tenant.</summary>
public sealed class MeritCyclePlan(string code, string label, int capacity, string reviewer, int attempts, string description, string currency, string unit, MeritCyclePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public MeritCyclePlanStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle plan in status created.</summary>
    public static MeritCyclePlan NewMeritCyclePlan(string code, string label)
    {
        return new MeritCyclePlan(code, label, 0, "", 0, "", "", "", MeritCyclePlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCyclePlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCyclePlanStatus next)
    {
        switch (Status)
        {
            case MeritCyclePlanStatus.Created:
                return next == MeritCyclePlanStatus.Published || next == MeritCyclePlanStatus.Retired;
            case MeritCyclePlanStatus.Validated:
                switch (next)
                {
                    case MeritCyclePlanStatus.Published:
                        return true;
                }
                return false;
            case MeritCyclePlanStatus.Published:
                switch (next)
                {
                    case MeritCyclePlanStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCyclePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Capacity - (double)Attempts, 2);
    }

    public MeritCyclePlan Copy()
    {
        return new MeritCyclePlan(Code, Label, Capacity, Reviewer, Attempts, Description, Currency, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
