namespace Acme.Workforce.Assets.Laptop.Window;

/// <summary>A rule a tiered laptop window must pass. Check returns its problems, or none.</summary>
public interface ITieredLaptopWindowRule
{
    List<string> Check(TieredLaptopWindow item);
}

public sealed record TieredLaptopWindowRequiredLabelRule(string Message) : ITieredLaptopWindowRule
{
    public List<string> Check(TieredLaptopWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopWindowCodePrefixRule(string Prefix) : ITieredLaptopWindowRule
{
    public List<string> Check(TieredLaptopWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopWindowWindowDaysRangeRule(int Min, int Max) : ITieredLaptopWindowRule
{
    public List<string> Check(TieredLaptopWindow item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredLaptopWindowRequiredSummaryRule(string Message) : ITieredLaptopWindowRule
{
    public List<string> Check(TieredLaptopWindow item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopWindowSummaryLengthRule(int MaxLength) : ITieredLaptopWindowRule
{
    public List<string> Check(TieredLaptopWindow item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopWindowCodeLengthRule(int MaxLength) : ITieredLaptopWindowRule
{
    public List<string> Check(TieredLaptopWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered laptop window.</summary>
public sealed record TieredLaptopWindowValidator(List<ITieredLaptopWindowRule> Rules)
{
    public List<string> Validate(TieredLaptopWindow item)
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

    public void RequireValid(TieredLaptopWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLaptopWindow> items)
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

/// <summary>Functions of tiered laptop window rules.</summary>
public static class TieredLaptopWindowRules
{
    public static TieredLaptopWindowValidator DefaultTieredLaptopWindowValidator()
    {
        return new TieredLaptopWindowValidator(
            new List<ITieredLaptopWindowRule>
            {
                new TieredLaptopWindowRequiredLabelRule("tiered laptop window needs an entry"),
                new TieredLaptopWindowCodePrefixRule("REF"),
                new TieredLaptopWindowWindowDaysRangeRule(80, 80),
                new TieredLaptopWindowRequiredSummaryRule("tiered laptop window needs a value"),
                new TieredLaptopWindowSummaryLengthRule(32),
                new TieredLaptopWindowCodeLengthRule(86400),
            });
    }
}
