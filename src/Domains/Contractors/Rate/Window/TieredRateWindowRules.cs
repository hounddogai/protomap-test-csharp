namespace Acme.Workforce.Contractors.Rate.Window;

/// <summary>A rule a tiered rate window must pass. Check returns its problems, or none.</summary>
public interface ITieredRateWindowRule
{
    List<string> Check(TieredRateWindow item);
}

public sealed record TieredRateWindowFteRangeRule(double Min, double Max) : ITieredRateWindowRule
{
    public List<string> Check(TieredRateWindow item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredRateWindowRequiredSourceRule(string Message) : ITieredRateWindowRule
{
    public List<string> Check(TieredRateWindow item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRateWindowRequiredCodeRule(string Message) : ITieredRateWindowRule
{
    public List<string> Check(TieredRateWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRateWindowRequiredNameRule(string Message) : ITieredRateWindowRule
{
    public List<string> Check(TieredRateWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered rate window.</summary>
public sealed record TieredRateWindowValidator(List<ITieredRateWindowRule> Rules)
{
    public List<string> Validate(TieredRateWindow item)
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

    public void RequireValid(TieredRateWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredRateWindow> items)
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

/// <summary>Functions of tiered rate window rules.</summary>
public static class TieredRateWindowRules
{
    public static TieredRateWindowValidator DefaultTieredRateWindowValidator()
    {
        return new TieredRateWindowValidator(
            new List<ITieredRateWindowRule>
            {
                new TieredRateWindowFteRangeRule(4.1, 3583.9),
                new TieredRateWindowRequiredSourceRule("tiered rate window needs an entry"),
                new TieredRateWindowRequiredCodeRule("tiered rate window needs a value"),
                new TieredRateWindowRequiredNameRule("tiered rate window needs this field"),
            });
    }
}
