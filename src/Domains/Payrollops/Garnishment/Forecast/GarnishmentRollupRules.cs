namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>A rule a garnishment rollup must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentRollupRule
{
    List<string> Check(GarnishmentRollup item);
}

public sealed record GarnishmentRollupRequiredLabelRule(string Message) : IGarnishmentRollupRule
{
    public List<string> Check(GarnishmentRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRollupTitleLengthRule(int MaxLength) : IGarnishmentRollupRule
{
    public List<string> Check(GarnishmentRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRollupCapacityRangeRule(int Min, int Max) : IGarnishmentRollupRule
{
    public List<string> Check(GarnishmentRollup item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a garnishment rollup.</summary>
public sealed record GarnishmentRollupValidator(List<IGarnishmentRollupRule> Rules)
{
    public List<string> Validate(GarnishmentRollup item)
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

    public void RequireValid(GarnishmentRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentRollup> items)
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

/// <summary>Functions of garnishment rollup rules.</summary>
public static class GarnishmentRollupRules
{
    public static GarnishmentRollupValidator DefaultGarnishmentRollupValidator()
    {
        return new GarnishmentRollupValidator(
            new List<IGarnishmentRollupRule>
            {
                new GarnishmentRollupRequiredLabelRule("garnishment rollup needs an entry"),
                new GarnishmentRollupTitleLengthRule(80),
                new GarnishmentRollupCapacityRangeRule(3600, 86400),
            });
    }
}
