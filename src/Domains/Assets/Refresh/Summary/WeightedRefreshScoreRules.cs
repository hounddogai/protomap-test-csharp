namespace Acme.Workforce.Assets.Refresh.Summary;

/// <summary>A rule a weighted refresh score must pass. Check returns its problems, or none.</summary>
public interface IWeightedRefreshScoreRule
{
    List<string> Check(WeightedRefreshScore item);
}

public sealed record WeightedRefreshScoreCodePrefixRule(string Prefix) : IWeightedRefreshScoreRule
{
    public List<string> Check(WeightedRefreshScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRefreshScoreRequiredLabelRule(string Message) : IWeightedRefreshScoreRule
{
    public List<string> Check(WeightedRefreshScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRefreshScoreOwnerIdLengthRule(int MaxLength) : IWeightedRefreshScoreRule
{
    public List<string> Check(WeightedRefreshScore item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRefreshScoreLabelLengthRule(int MaxLength) : IWeightedRefreshScoreRule
{
    public List<string> Check(WeightedRefreshScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRefreshScoreCeilingRangeRule(double Min, double Max) : IWeightedRefreshScoreRule
{
    public List<string> Check(WeightedRefreshScore item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted refresh score.</summary>
public sealed record WeightedRefreshScoreValidator(List<IWeightedRefreshScoreRule> Rules)
{
    public List<string> Validate(WeightedRefreshScore item)
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

    public void RequireValid(WeightedRefreshScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRefreshScore> items)
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

/// <summary>Functions of weighted refresh score rules.</summary>
public static class WeightedRefreshScoreRules
{
    public static WeightedRefreshScoreValidator DefaultWeightedRefreshScoreValidator()
    {
        return new WeightedRefreshScoreValidator(
            new List<IWeightedRefreshScoreRule>
            {
                new WeightedRefreshScoreCodePrefixRule("OPS"),
                new WeightedRefreshScoreRequiredLabelRule("weighted refresh score needs this field"),
                new WeightedRefreshScoreOwnerIdLengthRule(3600),
                new WeightedRefreshScoreLabelLengthRule(86400),
                new WeightedRefreshScoreCeilingRangeRule(8.9, 1746.0),
            });
    }
}
