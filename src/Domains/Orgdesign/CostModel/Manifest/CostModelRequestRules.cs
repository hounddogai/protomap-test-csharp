namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

/// <summary>A rule a cost model request must pass. Check returns its problems, or none.</summary>
public interface ICostModelRequestRule
{
    List<string> Check(CostModelRequest item);
}

public sealed record CostModelRequestCodePrefixRule(string Prefix) : ICostModelRequestRule
{
    public List<string> Check(CostModelRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelRequestCodeLengthRule(int MaxLength) : ICostModelRequestRule
{
    public List<string> Check(CostModelRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelRequestRetriesRangeRule(int Min, int Max) : ICostModelRequestRule
{
    public List<string> Check(CostModelRequest item)
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

public sealed record CostModelRequestStartsAtRangeRule(int Min, int Max) : ICostModelRequestRule
{
    public List<string> Check(CostModelRequest item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model request.</summary>
public sealed record CostModelRequestValidator(List<ICostModelRequestRule> Rules)
{
    public List<string> Validate(CostModelRequest item)
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

    public void RequireValid(CostModelRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelRequest> items)
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

/// <summary>Functions of cost model request rules.</summary>
public static class CostModelRequestRules
{
    public static CostModelRequestValidator DefaultCostModelRequestValidator()
    {
        return new CostModelRequestValidator(
            new List<ICostModelRequestRule>
            {
                new CostModelRequestCodePrefixRule("ACM"),
                new CostModelRequestCodeLengthRule(86400),
                new CostModelRequestRetriesRangeRule(120, 3600),
                new CostModelRequestStartsAtRangeRule(32, 80),
            });
    }
}
