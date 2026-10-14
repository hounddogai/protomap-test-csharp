namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant mapping is in its life cycle.</summary>
public enum EquityGrantMappingStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One equity grant mapping, with what the planners need to act on it.</summary>
public sealed class EquityGrantMapping(string code, string name, string category, string reviewer, double budget, EquityGrantMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public double Budget { get; set; } = budget;
    public EquityGrantMappingStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant mapping in status created.</summary>
    public static EquityGrantMapping NewEquityGrantMapping(string code, string name)
    {
        return new EquityGrantMapping(code, name, "", "", 0.0, EquityGrantMappingStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantMappingStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantMappingStatus next)
    {
        switch (Status)
        {
            case EquityGrantMappingStatus.Created:
                return next == EquityGrantMappingStatus.Published || next == EquityGrantMappingStatus.Retired;
            case EquityGrantMappingStatus.Validated:
                switch (next)
                {
                    case EquityGrantMappingStatus.Published:
                    case EquityGrantMappingStatus.Retired:
                        return true;
                }
                return false;
            case EquityGrantMappingStatus.Published:
                switch (next)
                {
                    case EquityGrantMappingStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public EquityGrantMapping Copy()
    {
        return new EquityGrantMapping(Code, Name, Category, Reviewer, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
