namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal pay band cycle must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayBandCycleRule
{
    List<string> Check(SeasonalPayBandCycle item);
}

public sealed record SeasonalPayBandCycleMarginRangeRule(double Min, double Max) : ISeasonalPayBandCycleRule
{
    public List<string> Check(SeasonalPayBandCycle item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPayBandCycleRequiredLabelRule(string Message) : ISeasonalPayBandCycleRule
{
    public List<string> Check(SeasonalPayBandCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandCycleAllowedChecksumRule(List<string> Allowed) : ISeasonalPayBandCycleRule
{
    public List<string> Check(SeasonalPayBandCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandCycleGraceDaysRangeRule(int Min, int Max) : ISeasonalPayBandCycleRule
{
    public List<string> Check(SeasonalPayBandCycle item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPayBandCycleTagsLimitRule(int MaxCount, List<string> Banned) : ISeasonalPayBandCycleRule
{
    public List<string> Check(SeasonalPayBandCycle item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal pay band cycle.</summary>
public sealed record SeasonalPayBandCycleValidator(List<ISeasonalPayBandCycleRule> Rules)
{
    public List<string> Validate(SeasonalPayBandCycle item)
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

    public void RequireValid(SeasonalPayBandCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayBandCycle> items)
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

/// <summary>Functions of seasonal pay band cycle rules.</summary>
public static class SeasonalPayBandCycleRules
{
    public static SeasonalPayBandCycleValidator DefaultSeasonalPayBandCycleValidator()
    {
        return new SeasonalPayBandCycleValidator(
            new List<ISeasonalPayBandCycleRule>
            {
                new SeasonalPayBandCycleMarginRangeRule(3.5, 1142.4),
                new SeasonalPayBandCycleRequiredLabelRule("seasonal pay band cycle needs a value"),
                new SeasonalPayBandCycleAllowedChecksumRule(new List<string> { "rolling", "capped", "seasonal" }),
                new SeasonalPayBandCycleGraceDaysRangeRule(16, 32),
                new SeasonalPayBandCycleTagsLimitRule(16, new List<string>()),
            });
    }
}
