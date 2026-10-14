namespace Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>A rule a laptop cycle must pass. Check returns its problems, or none.</summary>
public interface ILaptopCycleRule
{
    List<string> Check(LaptopCycle item);
}

public sealed record LaptopCycleMarginRangeRule(double Min, double Max) : ILaptopCycleRule
{
    public List<string> Check(LaptopCycle item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopCycleThresholdRangeRule(double Min, double Max) : ILaptopCycleRule
{
    public List<string> Check(LaptopCycle item)
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

public sealed record LaptopCycleRequiredSummaryRule(string Message) : ILaptopCycleRule
{
    public List<string> Check(LaptopCycle item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopCycleNameLengthRule(int MaxLength) : ILaptopCycleRule
{
    public List<string> Check(LaptopCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop cycle.</summary>
public sealed record LaptopCycleValidator(List<ILaptopCycleRule> Rules)
{
    public List<string> Validate(LaptopCycle item)
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

    public void RequireValid(LaptopCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopCycle> items)
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

/// <summary>Functions of laptop cycle rules.</summary>
public static class LaptopCycleRules
{
    public static LaptopCycleValidator DefaultLaptopCycleValidator()
    {
        return new LaptopCycleValidator(
            new List<ILaptopCycleRule>
            {
                new LaptopCycleMarginRangeRule(7.3, 3487.8),
                new LaptopCycleThresholdRangeRule(3.9, 4601.2),
                new LaptopCycleRequiredSummaryRule("laptop cycle needs an entry"),
                new LaptopCycleNameLengthRule(120),
            });
    }
}
