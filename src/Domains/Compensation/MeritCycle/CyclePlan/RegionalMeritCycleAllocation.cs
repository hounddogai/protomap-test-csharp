namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional merit cycle allocation is in its life cycle.</summary>
public enum RegionalMeritCycleAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional merit cycle allocation as the merit cycle team keeps it.</summary>
public sealed class RegionalMeritCycleAllocation(string code, string name, Dictionary<string, string> attributes, int revision, double threshold, double amount, int createdAt, string costCenter, double maximum, int expiresAt, RegionalMeritCycleAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public double Threshold { get; set; } = threshold;
    public double Amount { get; set; } = amount;
    public int CreatedAt { get; set; } = createdAt;
    public string CostCenter { get; set; } = costCenter;
    public double Maximum { get; set; } = maximum;
    public int ExpiresAt { get; set; } = expiresAt;
    public RegionalMeritCycleAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a regional merit cycle allocation in status created.</summary>
    public static RegionalMeritCycleAllocation NewRegionalMeritCycleAllocation(string code, string name)
    {
        return new RegionalMeritCycleAllocation(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            0.0,
            0.0,
            0,
            "",
            0.0,
            0,
            RegionalMeritCycleAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalMeritCycleAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalMeritCycleAllocationStatus next)
    {
        switch (Status)
        {
            case RegionalMeritCycleAllocationStatus.Created:
                return next == RegionalMeritCycleAllocationStatus.Validated || next == RegionalMeritCycleAllocationStatus.Retired;
            case RegionalMeritCycleAllocationStatus.Validated:
                switch (next)
                {
                    case RegionalMeritCycleAllocationStatus.Retired:
                        return true;
                }
                return false;
            case RegionalMeritCycleAllocationStatus.Published:
                return next == RegionalMeritCycleAllocationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalMeritCycleAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional merit cycle allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"regional merit cycle allocation"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round(Maximum - Threshold, 2);
    }

    public RegionalMeritCycleAllocation Copy()
    {
        return new RegionalMeritCycleAllocation(
            Code,
            Name,
            Attributes,
            Revision,
            Threshold,
            Amount,
            CreatedAt,
            CostCenter,
            Maximum,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
