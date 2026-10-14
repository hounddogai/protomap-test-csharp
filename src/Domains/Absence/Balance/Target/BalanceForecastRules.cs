namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance forecast must pass. Check returns its problems, or none.</summary>
public interface IBalanceForecastRule
{
    List<string> Check(BalanceForecast item);
}

public sealed record BalanceForecastCodePrefixRule(string Prefix) : IBalanceForecastRule
{
    public List<string> Check(BalanceForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceForecastMarginRangeRule(double Min, double Max) : IBalanceForecastRule
{
    public List<string> Check(BalanceForecast item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceForecastPhoneLengthRule(int MaxLength) : IBalanceForecastRule
{
    public List<string> Check(BalanceForecast item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceForecastAllowedCodeRule(List<string> Allowed) : IBalanceForecastRule
{
    public List<string> Check(BalanceForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record BalanceForecastVersionRangeRule(int Min, int Max) : IBalanceForecastRule
{
    public List<string> Check(BalanceForecast item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceForecastCodeLengthRule(int MaxLength) : IBalanceForecastRule
{
    public List<string> Check(BalanceForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance forecast.</summary>
public sealed record BalanceForecastValidator(List<IBalanceForecastRule> Rules)
{
    public List<string> Validate(BalanceForecast item)
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

    public void RequireValid(BalanceForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceForecast> items)
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

/// <summary>Functions of balance forecast rules.</summary>
public static class BalanceForecastRules
{
    public static BalanceForecastValidator DefaultBalanceForecastValidator()
    {
        return new BalanceForecastValidator(
            new List<IBalanceForecastRule>
            {
                new BalanceForecastCodePrefixRule("REF"),
                new BalanceForecastMarginRangeRule(3.3, 2008.2),
                new BalanceForecastPhoneLengthRule(80),
                new BalanceForecastAllowedCodeRule(new List<string> { "global" }),
                new BalanceForecastVersionRangeRule(64, 1000),
                new BalanceForecastCodeLengthRule(255),
            });
    }
}
