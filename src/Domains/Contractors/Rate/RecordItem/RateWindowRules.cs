namespace Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>A rule a rate window must pass. Check returns its problems, or none.</summary>
public interface IRateWindowRule
{
    List<string> Check(RateWindow item);
}

public sealed record RateWindowExpiresAtRangeRule(int Min, int Max) : IRateWindowRule
{
    public List<string> Check(RateWindow item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateWindowCodePrefixRule(string Prefix) : IRateWindowRule
{
    public List<string> Check(RateWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateWindowAmountRangeRule(double Min, double Max) : IRateWindowRule
{
    public List<string> Check(RateWindow item)
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

public sealed record RateWindowRequiredCodeRule(string Message) : IRateWindowRule
{
    public List<string> Check(RateWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateWindowCapacityRangeRule(int Min, int Max) : IRateWindowRule
{
    public List<string> Check(RateWindow item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rate window.</summary>
public sealed record RateWindowValidator(List<IRateWindowRule> Rules)
{
    public List<string> Validate(RateWindow item)
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

    public void RequireValid(RateWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateWindow> items)
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

/// <summary>Functions of rate window rules.</summary>
public static class RateWindowRules
{
    public static RateWindowValidator DefaultRateWindowValidator()
    {
        return new RateWindowValidator(
            new List<IRateWindowRule>
            {
                new RateWindowExpiresAtRangeRule(64, 1000),
                new RateWindowCodePrefixRule("ACM"),
                new RateWindowAmountRangeRule(6.5, 2286.7),
                new RateWindowRequiredCodeRule("rate window needs an entry"),
                new RateWindowCapacityRangeRule(80, 3600),
            });
    }
}
