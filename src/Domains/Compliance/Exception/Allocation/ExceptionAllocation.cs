namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception allocation is in its life cycle.</summary>
public enum ExceptionAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a exception allocation and its current status.</summary>
public sealed class ExceptionAllocation(string code, string name, string currency, string skill, string team, string unit, double amount, double ratio, double rating, double ceiling, Dictionary<string, string> attributes, ExceptionAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public string Team { get; set; } = team;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public double Ratio { get; set; } = ratio;
    public double Rating { get; set; } = rating;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public ExceptionAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a exception allocation in status idle.</summary>
    public static ExceptionAllocation NewExceptionAllocation(string code, string name)
    {
        return new ExceptionAllocation(
            code,
            name,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            ExceptionAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionAllocationStatus next)
    {
        switch (Status)
        {
            case ExceptionAllocationStatus.Idle:
                return next == ExceptionAllocationStatus.Warming;
            case ExceptionAllocationStatus.Warming:
                switch (next)
                {
                    case ExceptionAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case ExceptionAllocationStatus.Ready:
                return next == ExceptionAllocationStatus.Draining || next == ExceptionAllocationStatus.Stopped;
            case ExceptionAllocationStatus.Draining:
                switch (next)
                {
                    case ExceptionAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double ProjectedValue()
    {
        return Math.Round(Ceiling - Amount, 2);
    }

    public ExceptionAllocation Copy()
    {
        return new ExceptionAllocation(
            Code,
            Name,
            Currency,
            Skill,
            Team,
            Unit,
            Amount,
            Ratio,
            Rating,
            Ceiling,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
