namespace Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>A rule a pay band baseline must pass. Check returns its problems, or none.</summary>
public interface IPayBandBaselineRule
{
    List<string> Check(PayBandBaseline item);
}

public sealed record PayBandBaselineRequiredTitleRule(string Message) : IPayBandBaselineRule
{
    public List<string> Check(PayBandBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandBaselineCodePrefixRule(string Prefix) : IPayBandBaselineRule
{
    public List<string> Check(PayBandBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandBaselineCeilingRangeRule(double Min, double Max) : IPayBandBaselineRule
{
    public List<string> Check(PayBandBaseline item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band baseline.</summary>
public sealed record PayBandBaselineValidator(List<IPayBandBaselineRule> Rules)
{
    public List<string> Validate(PayBandBaseline item)
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

    public void RequireValid(PayBandBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandBaseline> items)
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

/// <summary>Functions of pay band baseline rules.</summary>
public static class PayBandBaselineRules
{
    public static PayBandBaselineValidator DefaultPayBandBaselineValidator()
    {
        return new PayBandBaselineValidator(
            new List<IPayBandBaselineRule>
            {
                new PayBandBaselineRequiredTitleRule("pay band baseline needs a value"),
                new PayBandBaselineCodePrefixRule("Q"),
                new PayBandBaselineCeilingRangeRule(1.7, 3586.6),
            });
    }
}
