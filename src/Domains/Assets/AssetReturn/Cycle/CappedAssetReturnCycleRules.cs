namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped asset return cycle must pass. Check returns its problems, or none.</summary>
public interface ICappedAssetReturnCycleRule
{
    List<string> Check(CappedAssetReturnCycle item);
}

public sealed record CappedAssetReturnCycleNameLengthRule(int MaxLength) : ICappedAssetReturnCycleRule
{
    public List<string> Check(CappedAssetReturnCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnCycleCodePrefixRule(string Prefix) : ICappedAssetReturnCycleRule
{
    public List<string> Check(CappedAssetReturnCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnCycleMaximumRangeRule(double Min, double Max) : ICappedAssetReturnCycleRule
{
    public List<string> Check(CappedAssetReturnCycle item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAssetReturnCycleAllowedJobCodeRule(List<string> Allowed) : ICappedAssetReturnCycleRule
{
    public List<string> Check(CappedAssetReturnCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnCycleJobCodeLengthRule(int MaxLength) : ICappedAssetReturnCycleRule
{
    public List<string> Check(CappedAssetReturnCycle item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnCycleBudgetRangeRule(double Min, double Max) : ICappedAssetReturnCycleRule
{
    public List<string> Check(CappedAssetReturnCycle item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped asset return cycle.</summary>
public sealed record CappedAssetReturnCycleValidator(List<ICappedAssetReturnCycleRule> Rules)
{
    public List<string> Validate(CappedAssetReturnCycle item)
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

    public void RequireValid(CappedAssetReturnCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAssetReturnCycle> items)
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

/// <summary>Functions of capped asset return cycle rules.</summary>
public static class CappedAssetReturnCycleRules
{
    public static CappedAssetReturnCycleValidator DefaultCappedAssetReturnCycleValidator()
    {
        return new CappedAssetReturnCycleValidator(
            new List<ICappedAssetReturnCycleRule>
            {
                new CappedAssetReturnCycleNameLengthRule(80),
                new CappedAssetReturnCycleCodePrefixRule("REF"),
                new CappedAssetReturnCycleMaximumRangeRule(0.8, 3636.9),
                new CappedAssetReturnCycleAllowedJobCodeRule(new List<string> { "tiered", "strict" }),
                new CappedAssetReturnCycleJobCodeLengthRule(86400),
                new CappedAssetReturnCycleBudgetRangeRule(3.8, 3571.8),
            });
    }
}
