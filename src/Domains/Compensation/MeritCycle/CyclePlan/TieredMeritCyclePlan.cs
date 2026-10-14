namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered merit cycle plan is in its life cycle.</summary>
public enum TieredMeritCyclePlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tiered merit cycle plan and its current status.</summary>
public sealed class TieredMeritCyclePlan(string code, string name, string managerId, double weight, Dictionary<string, string> attributes, string department, double budget, int limit, TieredMeritCyclePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Department { get; set; } = department;
    public double Budget { get; set; } = budget;
    public int Limit { get; set; } = limit;
    public TieredMeritCyclePlanStatus Status { get; set; } = status;

    /// <summary>Starts a tiered merit cycle plan in status created.</summary>
    public static TieredMeritCyclePlan NewTieredMeritCyclePlan(string code, string name)
    {
        return new TieredMeritCyclePlan(
            code,
            name,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            0.0,
            0,
            TieredMeritCyclePlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredMeritCyclePlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredMeritCyclePlanStatus next)
    {
        switch (Status)
        {
            case TieredMeritCyclePlanStatus.Created:
                return next == TieredMeritCyclePlanStatus.Validated || next == TieredMeritCyclePlanStatus.Retired;
            case TieredMeritCyclePlanStatus.Validated:
                switch (next)
                {
                    case TieredMeritCyclePlanStatus.Published:
                    case TieredMeritCyclePlanStatus.Retired:
                        return true;
                }
                return false;
            case TieredMeritCyclePlanStatus.Published:
                return next == TieredMeritCyclePlanStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredMeritCyclePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered merit cycle plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered merit cycle plan"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Budget + Weight, 2);
    }

    public TieredMeritCyclePlan Copy()
    {
        return new TieredMeritCyclePlan(
            Code,
            Name,
            ManagerId,
            Weight,
            Attributes,
            Department,
            Budget,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
