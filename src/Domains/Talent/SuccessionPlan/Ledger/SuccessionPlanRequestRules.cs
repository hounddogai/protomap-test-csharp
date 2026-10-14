namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>A rule a succession plan request must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanRequestRule
{
    List<string> Check(SuccessionPlanRequest item);
}

public sealed record SuccessionPlanRequestCodeLengthRule(int MaxLength) : ISuccessionPlanRequestRule
{
    public List<string> Check(SuccessionPlanRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRequestBatchSizeRangeRule(int Min, int Max) : ISuccessionPlanRequestRule
{
    public List<string> Check(SuccessionPlanRequest item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanRequestExternalRefLengthRule(int MaxLength) : ISuccessionPlanRequestRule
{
    public List<string> Check(SuccessionPlanRequest item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRequestCodePrefixRule(string Prefix) : ISuccessionPlanRequestRule
{
    public List<string> Check(SuccessionPlanRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRequestCostCenterLengthRule(int MaxLength) : ISuccessionPlanRequestRule
{
    public List<string> Check(SuccessionPlanRequest item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRequestMinimumRangeRule(double Min, double Max) : ISuccessionPlanRequestRule
{
    public List<string> Check(SuccessionPlanRequest item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan request.</summary>
public sealed record SuccessionPlanRequestValidator(List<ISuccessionPlanRequestRule> Rules)
{
    public List<string> Validate(SuccessionPlanRequest item)
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

    public void RequireValid(SuccessionPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanRequest> items)
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

/// <summary>Functions of succession plan request rules.</summary>
public static class SuccessionPlanRequestRules
{
    public static SuccessionPlanRequestValidator DefaultSuccessionPlanRequestValidator()
    {
        return new SuccessionPlanRequestValidator(
            new List<ISuccessionPlanRequestRule>
            {
                new SuccessionPlanRequestCodeLengthRule(80),
                new SuccessionPlanRequestBatchSizeRangeRule(16, 1000),
                new SuccessionPlanRequestExternalRefLengthRule(64),
                new SuccessionPlanRequestCodePrefixRule("ACM"),
                new SuccessionPlanRequestCostCenterLengthRule(64),
                new SuccessionPlanRequestMinimumRangeRule(4.4, 74.7),
            });
    }
}
