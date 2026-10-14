namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band forecast must pass. Check returns its problems, or none.</summary>
public interface IPayBandForecastRule
{
    List<string> Check(PayBandForecast item);
}

public sealed record PayBandForecastDepartmentLengthRule(int MaxLength) : IPayBandForecastRule
{
    public List<string> Check(PayBandForecast item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandForecastRatingRangeRule(double Min, double Max) : IPayBandForecastRule
{
    public List<string> Check(PayBandForecast item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandForecastCodePrefixRule(string Prefix) : IPayBandForecastRule
{
    public List<string> Check(PayBandForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandForecastLabelLengthRule(int MaxLength) : IPayBandForecastRule
{
    public List<string> Check(PayBandForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandForecastAllowedDepartmentRule(List<string> Allowed) : IPayBandForecastRule
{
    public List<string> Check(PayBandForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PayBandForecastRequiredDepartmentRule(string Message) : IPayBandForecastRule
{
    public List<string> Check(PayBandForecast item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band forecast.</summary>
public sealed record PayBandForecastValidator(List<IPayBandForecastRule> Rules)
{
    public List<string> Validate(PayBandForecast item)
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

    public void RequireValid(PayBandForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandForecast> items)
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

/// <summary>Functions of pay band forecast rules.</summary>
public static class PayBandForecastRules
{
    public static PayBandForecastValidator DefaultPayBandForecastValidator()
    {
        return new PayBandForecastValidator(
            new List<IPayBandForecastRule>
            {
                new PayBandForecastDepartmentLengthRule(16),
                new PayBandForecastRatingRangeRule(3.6, 1984.3),
                new PayBandForecastCodePrefixRule("REF"),
                new PayBandForecastLabelLengthRule(64),
                new PayBandForecastAllowedDepartmentRule(new List<string> { "weighted", "default", "fallback" }),
                new PayBandForecastRequiredDepartmentRule("pay band forecast needs an entry"),
            });
    }
}
