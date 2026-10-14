namespace Acme.Workforce.Contractors.Rate.Batch;

/// <summary>A rule a rate allocation must pass. Check returns its problems, or none.</summary>
public interface IRateAllocationRule
{
    List<string> Check(RateAllocation item);
}

public sealed record RateAllocationRequiredLabelRule(string Message) : IRateAllocationRule
{
    public List<string> Check(RateAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateAllocationRevisionRangeRule(int Min, int Max) : IRateAllocationRule
{
    public List<string> Check(RateAllocation item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RateAllocationRequiredChannelRule(string Message) : IRateAllocationRule
{
    public List<string> Check(RateAllocation item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate allocation.</summary>
public sealed record RateAllocationValidator(List<IRateAllocationRule> Rules)
{
    public List<string> Validate(RateAllocation item)
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

    public void RequireValid(RateAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateAllocation> items)
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

/// <summary>Functions of rate allocation rules.</summary>
public static class RateAllocationRules
{
    public static RateAllocationValidator DefaultRateAllocationValidator()
    {
        return new RateAllocationValidator(
            new List<IRateAllocationRule>
            {
                new RateAllocationRequiredLabelRule("rate allocation needs this field"),
                new RateAllocationRevisionRangeRule(32, 1000),
                new RateAllocationRequiredChannelRule("rate allocation needs an entry"),
            });
    }
}
