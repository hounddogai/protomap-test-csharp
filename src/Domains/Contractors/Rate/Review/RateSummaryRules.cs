namespace Acme.Workforce.Contractors.Rate.Review;

/// <summary>A rule a rate summary must pass. Check returns its problems, or none.</summary>
public interface IRateSummaryRule
{
    List<string> Check(RateSummary item);
}

public sealed record RateSummaryFloorValueRangeRule(double Min, double Max) : IRateSummaryRule
{
    public List<string> Check(RateSummary item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateSummaryTitleLengthRule(int MaxLength) : IRateSummaryRule
{
    public List<string> Check(RateSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateSummaryCodePrefixRule(string Prefix) : IRateSummaryRule
{
    public List<string> Check(RateSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateSummaryRequiredNameRule(string Message) : IRateSummaryRule
{
    public List<string> Check(RateSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate summary.</summary>
public sealed record RateSummaryValidator(List<IRateSummaryRule> Rules)
{
    public List<string> Validate(RateSummary item)
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

    public void RequireValid(RateSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateSummary> items)
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

/// <summary>Functions of rate summary rules.</summary>
public static class RateSummaryRules
{
    public static RateSummaryValidator DefaultRateSummaryValidator()
    {
        return new RateSummaryValidator(
            new List<IRateSummaryRule>
            {
                new RateSummaryFloorValueRangeRule(1.0, 3447.0),
                new RateSummaryTitleLengthRule(32),
                new RateSummaryCodePrefixRule("REF"),
                new RateSummaryRequiredNameRule("rate summary needs an entry"),
            });
    }
}
