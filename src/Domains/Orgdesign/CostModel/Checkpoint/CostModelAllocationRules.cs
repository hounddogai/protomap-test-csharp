namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>A rule a cost model allocation must pass. Check returns its problems, or none.</summary>
public interface ICostModelAllocationRule
{
    List<string> Check(CostModelAllocation item);
}

public sealed record CostModelAllocationCodePrefixRule(string Prefix) : ICostModelAllocationRule
{
    public List<string> Check(CostModelAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAllocationRequiredLocationRule(string Message) : ICostModelAllocationRule
{
    public List<string> Check(CostModelAllocation item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelAllocationJobCodeLengthRule(int MaxLength) : ICostModelAllocationRule
{
    public List<string> Check(CostModelAllocation item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAllocationLocationLengthRule(int MaxLength) : ICostModelAllocationRule
{
    public List<string> Check(CostModelAllocation item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model allocation.</summary>
public sealed record CostModelAllocationValidator(List<ICostModelAllocationRule> Rules)
{
    public List<string> Validate(CostModelAllocation item)
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

    public void RequireValid(CostModelAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelAllocation> items)
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

/// <summary>Functions of cost model allocation rules.</summary>
public static class CostModelAllocationRules
{
    public static CostModelAllocationValidator DefaultCostModelAllocationValidator()
    {
        return new CostModelAllocationValidator(
            new List<ICostModelAllocationRule>
            {
                new CostModelAllocationCodePrefixRule("ACM"),
                new CostModelAllocationRequiredLocationRule("cost model allocation needs this field"),
                new CostModelAllocationJobCodeLengthRule(120),
                new CostModelAllocationLocationLengthRule(1000),
            });
    }
}
