namespace Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>A rule a regional reorg score must pass. Check returns its problems, or none.</summary>
public interface IRegionalReorgScoreRule
{
    List<string> Check(RegionalReorgScore item);
}

public sealed record RegionalReorgScoreUpdatedAtRangeRule(int Min, int Max) : IRegionalReorgScoreRule
{
    public List<string> Check(RegionalReorgScore item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalReorgScoreRequiredNameRule(string Message) : IRegionalReorgScoreRule
{
    public List<string> Check(RegionalReorgScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgScoreRequiredSalaryBandRule(string Message) : IRegionalReorgScoreRule
{
    public List<string> Check(RegionalReorgScore item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgScoreCodePrefixRule(string Prefix) : IRegionalReorgScoreRule
{
    public List<string> Check(RegionalReorgScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgScoreSalaryBandLengthRule(int MaxLength) : IRegionalReorgScoreRule
{
    public List<string> Check(RegionalReorgScore item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgScoreCodeLengthRule(int MaxLength) : IRegionalReorgScoreRule
{
    public List<string> Check(RegionalReorgScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional reorg score.</summary>
public sealed record RegionalReorgScoreValidator(List<IRegionalReorgScoreRule> Rules)
{
    public List<string> Validate(RegionalReorgScore item)
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

    public void RequireValid(RegionalReorgScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalReorgScore> items)
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

/// <summary>Functions of regional reorg score rules.</summary>
public static class RegionalReorgScoreRules
{
    public static RegionalReorgScoreValidator DefaultRegionalReorgScoreValidator()
    {
        return new RegionalReorgScoreValidator(
            new List<IRegionalReorgScoreRule>
            {
                new RegionalReorgScoreUpdatedAtRangeRule(64, 80),
                new RegionalReorgScoreRequiredNameRule("regional reorg score needs an entry"),
                new RegionalReorgScoreRequiredSalaryBandRule("regional reorg score needs a value"),
                new RegionalReorgScoreCodePrefixRule("OPS"),
                new RegionalReorgScoreSalaryBandLengthRule(3600),
                new RegionalReorgScoreCodeLengthRule(255),
            });
    }
}
