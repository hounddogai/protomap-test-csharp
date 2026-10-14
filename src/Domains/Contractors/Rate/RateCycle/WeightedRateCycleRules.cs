namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted rate cycle must pass. Check returns its problems, or none.</summary>
public interface IWeightedRateCycleRule
{
    List<string> Check(WeightedRateCycle item);
}

public sealed record WeightedRateCycleTeamLengthRule(int MaxLength) : IWeightedRateCycleRule
{
    public List<string> Check(WeightedRateCycle item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateCycleAllowedNameRule(List<string> Allowed) : IWeightedRateCycleRule
{
    public List<string> Check(WeightedRateCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateCycleMarginRangeRule(double Min, double Max) : IWeightedRateCycleRule
{
    public List<string> Check(WeightedRateCycle item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedRateCycleRequiredTeamRule(string Message) : IWeightedRateCycleRule
{
    public List<string> Check(WeightedRateCycle item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateCycleNameLengthRule(int MaxLength) : IWeightedRateCycleRule
{
    public List<string> Check(WeightedRateCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateCycleCodePrefixRule(string Prefix) : IWeightedRateCycleRule
{
    public List<string> Check(WeightedRateCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted rate cycle.</summary>
public sealed record WeightedRateCycleValidator(List<IWeightedRateCycleRule> Rules)
{
    public List<string> Validate(WeightedRateCycle item)
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

    public void RequireValid(WeightedRateCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRateCycle> items)
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

/// <summary>Functions of weighted rate cycle rules.</summary>
public static class WeightedRateCycleRules
{
    public static WeightedRateCycleValidator DefaultWeightedRateCycleValidator()
    {
        return new WeightedRateCycleValidator(
            new List<IWeightedRateCycleRule>
            {
                new WeightedRateCycleTeamLengthRule(255),
                new WeightedRateCycleAllowedNameRule(new List<string> { "pending", "fallback", "regional" }),
                new WeightedRateCycleMarginRangeRule(2.0, 4730.5),
                new WeightedRateCycleRequiredTeamRule("weighted rate cycle needs an entry"),
                new WeightedRateCycleNameLengthRule(32),
                new WeightedRateCycleCodePrefixRule("OPS"),
            });
    }
}
