namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>A rule a regional merit cycle slot must pass. Check returns its problems, or none.</summary>
public interface IRegionalMeritCycleSlotRule
{
    List<string> Check(RegionalMeritCycleSlot item);
}

public sealed record RegionalMeritCycleSlotCodePrefixRule(string Prefix) : IRegionalMeritCycleSlotRule
{
    public List<string> Check(RegionalMeritCycleSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalMeritCycleSlotSalaryBandLengthRule(int MaxLength) : IRegionalMeritCycleSlotRule
{
    public List<string> Check(RegionalMeritCycleSlot item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalMeritCycleSlotAmountRangeRule(double Min, double Max) : IRegionalMeritCycleSlotRule
{
    public List<string> Check(RegionalMeritCycleSlot item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional merit cycle slot.</summary>
public sealed record RegionalMeritCycleSlotValidator(List<IRegionalMeritCycleSlotRule> Rules)
{
    public List<string> Validate(RegionalMeritCycleSlot item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(RegionalMeritCycleSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalMeritCycleSlot> items)
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

/// <summary>Functions of regional merit cycle slot rules.</summary>
public static class RegionalMeritCycleSlotRules
{
    public static RegionalMeritCycleSlotValidator DefaultRegionalMeritCycleSlotValidator()
    {
        return new RegionalMeritCycleSlotValidator(
            new List<IRegionalMeritCycleSlotRule>
            {
                new RegionalMeritCycleSlotCodePrefixRule("Q"),
                new RegionalMeritCycleSlotSalaryBandLengthRule(1000),
                new RegionalMeritCycleSlotAmountRangeRule(8.6, 3477.1),
            });
    }
}
