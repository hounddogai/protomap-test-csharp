namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict accrual slot must pass. Check returns its problems, or none.</summary>
public interface IStrictAccrualSlotRule
{
    List<string> Check(StrictAccrualSlot item);
}

public sealed record StrictAccrualSlotRetriesRangeRule(int Min, int Max) : IStrictAccrualSlotRule
{
    public List<string> Check(StrictAccrualSlot item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAccrualSlotCodePrefixRule(string Prefix) : IStrictAccrualSlotRule
{
    public List<string> Check(StrictAccrualSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualSlotAllowedCostCenterRule(List<string> Allowed) : IStrictAccrualSlotRule
{
    public List<string> Check(StrictAccrualSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is stale" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualSlotThresholdRangeRule(double Min, double Max) : IStrictAccrualSlotRule
{
    public List<string> Check(StrictAccrualSlot item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAccrualSlotDepartmentLengthRule(int MaxLength) : IStrictAccrualSlotRule
{
    public List<string> Check(StrictAccrualSlot item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualSlotLabelLengthRule(int MaxLength) : IStrictAccrualSlotRule
{
    public List<string> Check(StrictAccrualSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict accrual slot.</summary>
public sealed record StrictAccrualSlotValidator(List<IStrictAccrualSlotRule> Rules)
{
    public List<string> Validate(StrictAccrualSlot item)
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

    public void RequireValid(StrictAccrualSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAccrualSlot> items)
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

/// <summary>Functions of strict accrual slot rules.</summary>
public static class StrictAccrualSlotRules
{
    public static StrictAccrualSlotValidator DefaultStrictAccrualSlotValidator()
    {
        return new StrictAccrualSlotValidator(
            new List<IStrictAccrualSlotRule>
            {
                new StrictAccrualSlotRetriesRangeRule(64, 86400),
                new StrictAccrualSlotCodePrefixRule("REF"),
                new StrictAccrualSlotAllowedCostCenterRule(new List<string> { "tiered", "capped" }),
                new StrictAccrualSlotThresholdRangeRule(5.2, 3566.7),
                new StrictAccrualSlotDepartmentLengthRule(86400),
                new StrictAccrualSlotLabelLengthRule(86400),
            });
    }
}
