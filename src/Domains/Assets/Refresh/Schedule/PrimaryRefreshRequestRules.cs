namespace Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>A rule a primary refresh request must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRefreshRequestRule
{
    List<string> Check(PrimaryRefreshRequest item);
}

public sealed record PrimaryRefreshRequestNameLengthRule(int MaxLength) : IPrimaryRefreshRequestRule
{
    public List<string> Check(PrimaryRefreshRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshRequestRequiredSourceRule(string Message) : IPrimaryRefreshRequestRule
{
    public List<string> Check(PrimaryRefreshRequest item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshRequestCapacityRangeRule(int Min, int Max) : IPrimaryRefreshRequestRule
{
    public List<string> Check(PrimaryRefreshRequest item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary refresh request.</summary>
public sealed record PrimaryRefreshRequestValidator(List<IPrimaryRefreshRequestRule> Rules)
{
    public List<string> Validate(PrimaryRefreshRequest item)
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

    public void RequireValid(PrimaryRefreshRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRefreshRequest> items)
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

/// <summary>Functions of primary refresh request rules.</summary>
public static class PrimaryRefreshRequestRules
{
    public static PrimaryRefreshRequestValidator DefaultPrimaryRefreshRequestValidator()
    {
        return new PrimaryRefreshRequestValidator(
            new List<IPrimaryRefreshRequestRule>
            {
                new PrimaryRefreshRequestNameLengthRule(120),
                new PrimaryRefreshRequestRequiredSourceRule("primary refresh request needs a value"),
                new PrimaryRefreshRequestCapacityRangeRule(16, 86400),
            });
    }
}
