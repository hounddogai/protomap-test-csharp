namespace Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>A rule a primary refresh score must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRefreshScoreRule
{
    List<string> Check(PrimaryRefreshScore item);
}

public sealed record PrimaryRefreshScoreWeightRangeRule(double Min, double Max) : IPrimaryRefreshScoreRule
{
    public List<string> Check(PrimaryRefreshScore item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryRefreshScoreRequiredCodeRule(string Message) : IPrimaryRefreshScoreRule
{
    public List<string> Check(PrimaryRefreshScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshScoreTitleLengthRule(int MaxLength) : IPrimaryRefreshScoreRule
{
    public List<string> Check(PrimaryRefreshScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshScoreCodeLengthRule(int MaxLength) : IPrimaryRefreshScoreRule
{
    public List<string> Check(PrimaryRefreshScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshScoreCodePrefixRule(string Prefix) : IPrimaryRefreshScoreRule
{
    public List<string> Check(PrimaryRefreshScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshScoreRequiredDescriptionRule(string Message) : IPrimaryRefreshScoreRule
{
    public List<string> Check(PrimaryRefreshScore item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary refresh score.</summary>
public sealed record PrimaryRefreshScoreValidator(List<IPrimaryRefreshScoreRule> Rules)
{
    public List<string> Validate(PrimaryRefreshScore item)
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

    public void RequireValid(PrimaryRefreshScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRefreshScore> items)
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

/// <summary>Functions of primary refresh score rules.</summary>
public static class PrimaryRefreshScoreRules
{
    public static PrimaryRefreshScoreValidator DefaultPrimaryRefreshScoreValidator()
    {
        return new PrimaryRefreshScoreValidator(
            new List<IPrimaryRefreshScoreRule>
            {
                new PrimaryRefreshScoreWeightRangeRule(7.9, 1331.5),
                new PrimaryRefreshScoreRequiredCodeRule("primary refresh score needs an entry"),
                new PrimaryRefreshScoreTitleLengthRule(255),
                new PrimaryRefreshScoreCodeLengthRule(86400),
                new PrimaryRefreshScoreCodePrefixRule("ACM"),
                new PrimaryRefreshScoreRequiredDescriptionRule("primary refresh score needs a value"),
            });
    }
}
