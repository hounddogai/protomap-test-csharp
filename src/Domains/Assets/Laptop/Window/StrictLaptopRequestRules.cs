namespace Acme.Workforce.Assets.Laptop.Window;

/// <summary>A rule a strict laptop request must pass. Check returns its problems, or none.</summary>
public interface IStrictLaptopRequestRule
{
    List<string> Check(StrictLaptopRequest item);
}

public sealed record StrictLaptopRequestAmountRangeRule(double Min, double Max) : IStrictLaptopRequestRule
{
    public List<string> Check(StrictLaptopRequest item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopRequestRequiredSegmentRule(string Message) : IStrictLaptopRequestRule
{
    public List<string> Check(StrictLaptopRequest item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopRequestRetriesRangeRule(int Min, int Max) : IStrictLaptopRequestRule
{
    public List<string> Check(StrictLaptopRequest item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopRequestRequiredCodeRule(string Message) : IStrictLaptopRequestRule
{
    public List<string> Check(StrictLaptopRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopRequestFteRangeRule(double Min, double Max) : IStrictLaptopRequestRule
{
    public List<string> Check(StrictLaptopRequest item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict laptop request.</summary>
public sealed record StrictLaptopRequestValidator(List<IStrictLaptopRequestRule> Rules)
{
    public List<string> Validate(StrictLaptopRequest item)
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

    public void RequireValid(StrictLaptopRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictLaptopRequest> items)
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

/// <summary>Functions of strict laptop request rules.</summary>
public static class StrictLaptopRequestRules
{
    public static StrictLaptopRequestValidator DefaultStrictLaptopRequestValidator()
    {
        return new StrictLaptopRequestValidator(
            new List<IStrictLaptopRequestRule>
            {
                new StrictLaptopRequestAmountRangeRule(1.8, 4926.1),
                new StrictLaptopRequestRequiredSegmentRule("strict laptop request needs an entry"),
                new StrictLaptopRequestRetriesRangeRule(32, 255),
                new StrictLaptopRequestRequiredCodeRule("strict laptop request needs a value"),
                new StrictLaptopRequestFteRangeRule(5.6, 2593.5),
            });
    }
}
