namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>A rule a global pay band baseline must pass. Check returns its problems, or none.</summary>
public interface IGlobalPayBandBaselineRule
{
    List<string> Check(GlobalPayBandBaseline item);
}

public sealed record GlobalPayBandBaselineRequiredCodeRule(string Message) : IGlobalPayBandBaselineRule
{
    public List<string> Check(GlobalPayBandBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayBandBaselineCodeLengthRule(int MaxLength) : IGlobalPayBandBaselineRule
{
    public List<string> Check(GlobalPayBandBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayBandBaselineCodePrefixRule(string Prefix) : IGlobalPayBandBaselineRule
{
    public List<string> Check(GlobalPayBandBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayBandBaselineRequiredNameRule(string Message) : IGlobalPayBandBaselineRule
{
    public List<string> Check(GlobalPayBandBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global pay band baseline.</summary>
public sealed record GlobalPayBandBaselineValidator(List<IGlobalPayBandBaselineRule> Rules)
{
    public List<string> Validate(GlobalPayBandBaseline item)
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

    public void RequireValid(GlobalPayBandBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPayBandBaseline> items)
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

/// <summary>Functions of global pay band baseline rules.</summary>
public static class GlobalPayBandBaselineRules
{
    public static GlobalPayBandBaselineValidator DefaultGlobalPayBandBaselineValidator()
    {
        return new GlobalPayBandBaselineValidator(
            new List<IGlobalPayBandBaselineRule>
            {
                new GlobalPayBandBaselineRequiredCodeRule("global pay band baseline needs a value"),
                new GlobalPayBandBaselineCodeLengthRule(16),
                new GlobalPayBandBaselineCodePrefixRule("X"),
                new GlobalPayBandBaselineRequiredNameRule("global pay band baseline needs an entry"),
            });
    }
}
