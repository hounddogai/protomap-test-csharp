namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>A rule a tiered training due variance must pass. Check returns its problems, or none.</summary>
public interface ITieredTrainingDueVarianceRule
{
    List<string> Check(TieredTrainingDueVariance item);
}

public sealed record TieredTrainingDueVarianceSegmentLengthRule(int MaxLength) : ITieredTrainingDueVarianceRule
{
    public List<string> Check(TieredTrainingDueVariance item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueVarianceCodePrefixRule(string Prefix) : ITieredTrainingDueVarianceRule
{
    public List<string> Check(TieredTrainingDueVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueVarianceCodeLengthRule(int MaxLength) : ITieredTrainingDueVarianceRule
{
    public List<string> Check(TieredTrainingDueVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueVarianceDescriptionLengthRule(int MaxLength) : ITieredTrainingDueVarianceRule
{
    public List<string> Check(TieredTrainingDueVariance item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueVarianceTitleLengthRule(int MaxLength) : ITieredTrainingDueVarianceRule
{
    public List<string> Check(TieredTrainingDueVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueVarianceFteRangeRule(double Min, double Max) : ITieredTrainingDueVarianceRule
{
    public List<string> Check(TieredTrainingDueVariance item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered training due variance.</summary>
public sealed record TieredTrainingDueVarianceValidator(List<ITieredTrainingDueVarianceRule> Rules)
{
    public List<string> Validate(TieredTrainingDueVariance item)
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

    public void RequireValid(TieredTrainingDueVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredTrainingDueVariance> items)
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

/// <summary>Functions of tiered training due variance rules.</summary>
public static class TieredTrainingDueVarianceRules
{
    public static TieredTrainingDueVarianceValidator DefaultTieredTrainingDueVarianceValidator()
    {
        return new TieredTrainingDueVarianceValidator(
            new List<ITieredTrainingDueVarianceRule>
            {
                new TieredTrainingDueVarianceSegmentLengthRule(80),
                new TieredTrainingDueVarianceCodePrefixRule("OPS"),
                new TieredTrainingDueVarianceCodeLengthRule(1000),
                new TieredTrainingDueVarianceDescriptionLengthRule(86400),
                new TieredTrainingDueVarianceTitleLengthRule(64),
                new TieredTrainingDueVarianceFteRangeRule(2.4, 974.7),
            });
    }
}
