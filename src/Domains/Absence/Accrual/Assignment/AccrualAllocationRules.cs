namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual allocation must pass. Check returns its problems, or none.</summary>
public interface IAccrualAllocationRule
{
    List<string> Check(AccrualAllocation item);
}

public sealed record AccrualAllocationUnitLengthRule(int MaxLength) : IAccrualAllocationRule
{
    public List<string> Check(AccrualAllocation item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualAllocationRequiredLocationRule(string Message) : IAccrualAllocationRule
{
    public List<string> Check(AccrualAllocation item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualAllocationRequiredUnitRule(string Message) : IAccrualAllocationRule
{
    public List<string> Check(AccrualAllocation item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualAllocationAllowedSalaryBandRule(List<string> Allowed) : IAccrualAllocationRule
{
    public List<string> Check(AccrualAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AccrualAllocationAmountRangeRule(double Min, double Max) : IAccrualAllocationRule
{
    public List<string> Check(AccrualAllocation item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualAllocationLocationLengthRule(int MaxLength) : IAccrualAllocationRule
{
    public List<string> Check(AccrualAllocation item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual allocation.</summary>
public sealed record AccrualAllocationValidator(List<IAccrualAllocationRule> Rules)
{
    public List<string> Validate(AccrualAllocation item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(AccrualAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualAllocation> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of accrual allocation rules.</summary>
public static class AccrualAllocationRules
{
    public static AccrualAllocationValidator DefaultAccrualAllocationValidator()
    {
        return new AccrualAllocationValidator(
            new List<IAccrualAllocationRule>
            {
                new AccrualAllocationUnitLengthRule(64),
                new AccrualAllocationRequiredLocationRule("accrual allocation needs an entry"),
                new AccrualAllocationRequiredUnitRule("accrual allocation needs this field"),
                new AccrualAllocationAllowedSalaryBandRule(new List<string> { "batched" }),
                new AccrualAllocationAmountRangeRule(9.9, 1136.6),
                new AccrualAllocationLocationLengthRule(86400),
            });
    }
}
