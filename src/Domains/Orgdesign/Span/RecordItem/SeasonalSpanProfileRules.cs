namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal span profile must pass. Check returns its problems, or none.</summary>
public interface ISeasonalSpanProfileRule
{
    List<string> Check(SeasonalSpanProfile item);
}

public sealed record SeasonalSpanProfileCodeLengthRule(int MaxLength) : ISeasonalSpanProfileRule
{
    public List<string> Check(SeasonalSpanProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSpanProfileMaximumRangeRule(double Min, double Max) : ISeasonalSpanProfileRule
{
    public List<string> Check(SeasonalSpanProfile item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalSpanProfileAllowedCodeRule(List<string> Allowed) : ISeasonalSpanProfileRule
{
    public List<string> Check(SeasonalSpanProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal span profile.</summary>
public sealed record SeasonalSpanProfileValidator(List<ISeasonalSpanProfileRule> Rules)
{
    public List<string> Validate(SeasonalSpanProfile item)
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

    public void RequireValid(SeasonalSpanProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalSpanProfile> items)
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

/// <summary>Functions of seasonal span profile rules.</summary>
public static class SeasonalSpanProfileRules
{
    public static SeasonalSpanProfileValidator DefaultSeasonalSpanProfileValidator()
    {
        return new SeasonalSpanProfileValidator(
            new List<ISeasonalSpanProfileRule>
            {
                new SeasonalSpanProfileCodeLengthRule(1000),
                new SeasonalSpanProfileMaximumRangeRule(6.7, 4663.0),
                new SeasonalSpanProfileAllowedCodeRule(new List<string> { "regional" }),
            });
    }
}
