namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>A rule a reorg variance must pass. Check returns its problems, or none.</summary>
public interface IReorgVarianceRule
{
    List<string> Check(ReorgVariance item);
}

public sealed record ReorgVarianceCodePrefixRule(string Prefix) : IReorgVarianceRule
{
    public List<string> Check(ReorgVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgVarianceFloorValueRangeRule(double Min, double Max) : IReorgVarianceRule
{
    public List<string> Check(ReorgVariance item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgVarianceTitleLengthRule(int MaxLength) : IReorgVarianceRule
{
    public List<string> Check(ReorgVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg variance.</summary>
public sealed record ReorgVarianceValidator(List<IReorgVarianceRule> Rules)
{
    public List<string> Validate(ReorgVariance item)
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

    public void RequireValid(ReorgVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgVariance> items)
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

/// <summary>Functions of reorg variance rules.</summary>
public static class ReorgVarianceRules
{
    public static ReorgVarianceValidator DefaultReorgVarianceValidator()
    {
        return new ReorgVarianceValidator(
            new List<IReorgVarianceRule>
            {
                new ReorgVarianceCodePrefixRule("OPS"),
                new ReorgVarianceFloorValueRangeRule(7.2, 3788.8),
                new ReorgVarianceTitleLengthRule(255),
            });
    }
}
