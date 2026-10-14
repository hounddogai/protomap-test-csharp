namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>A rule a fallback cost model rollup must pass. Check returns its problems, or none.</summary>
public interface IFallbackCostModelRollupRule
{
    List<string> Check(FallbackCostModelRollup item);
}

public sealed record FallbackCostModelRollupRequiredCodeRule(string Message) : IFallbackCostModelRollupRule
{
    public List<string> Check(FallbackCostModelRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelRollupRequiredSourceRule(string Message) : IFallbackCostModelRollupRule
{
    public List<string> Check(FallbackCostModelRollup item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelRollupRequiredNameRule(string Message) : IFallbackCostModelRollupRule
{
    public List<string> Check(FallbackCostModelRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelRollupCodeLengthRule(int MaxLength) : IFallbackCostModelRollupRule
{
    public List<string> Check(FallbackCostModelRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback cost model rollup.</summary>
public sealed record FallbackCostModelRollupValidator(List<IFallbackCostModelRollupRule> Rules)
{
    public List<string> Validate(FallbackCostModelRollup item)
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

    public void RequireValid(FallbackCostModelRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackCostModelRollup> items)
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

/// <summary>Functions of fallback cost model rollup rules.</summary>
public static class FallbackCostModelRollupRules
{
    public static FallbackCostModelRollupValidator DefaultFallbackCostModelRollupValidator()
    {
        return new FallbackCostModelRollupValidator(
            new List<IFallbackCostModelRollupRule>
            {
                new FallbackCostModelRollupRequiredCodeRule("fallback cost model rollup needs an entry"),
                new FallbackCostModelRollupRequiredSourceRule("fallback cost model rollup needs an entry"),
                new FallbackCostModelRollupRequiredNameRule("fallback cost model rollup needs a value"),
                new FallbackCostModelRollupCodeLengthRule(80),
            });
    }
}
