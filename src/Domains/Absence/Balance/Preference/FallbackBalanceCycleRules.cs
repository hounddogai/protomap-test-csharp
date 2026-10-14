namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback balance cycle must pass. Check returns its problems, or none.</summary>
public interface IFallbackBalanceCycleRule
{
    List<string> Check(FallbackBalanceCycle item);
}

public sealed record FallbackBalanceCycleAllowedLabelRule(List<string> Allowed) : IFallbackBalanceCycleRule
{
    public List<string> Check(FallbackBalanceCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceCycleLabelLengthRule(int MaxLength) : IFallbackBalanceCycleRule
{
    public List<string> Check(FallbackBalanceCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceCycleAmountRangeRule(double Min, double Max) : IFallbackBalanceCycleRule
{
    public List<string> Check(FallbackBalanceCycle item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback balance cycle.</summary>
public sealed record FallbackBalanceCycleValidator(List<IFallbackBalanceCycleRule> Rules)
{
    public List<string> Validate(FallbackBalanceCycle item)
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

    public void RequireValid(FallbackBalanceCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBalanceCycle> items)
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

/// <summary>Functions of fallback balance cycle rules.</summary>
public static class FallbackBalanceCycleRules
{
    public static FallbackBalanceCycleValidator DefaultFallbackBalanceCycleValidator()
    {
        return new FallbackBalanceCycleValidator(
            new List<IFallbackBalanceCycleRule>
            {
                new FallbackBalanceCycleAllowedLabelRule(new List<string>()),
                new FallbackBalanceCycleLabelLengthRule(16),
                new FallbackBalanceCycleAmountRangeRule(5.1, 1739.2),
            });
    }
}
