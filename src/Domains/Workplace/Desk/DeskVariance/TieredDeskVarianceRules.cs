namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered desk variance must pass. Check returns its problems, or none.</summary>
public interface ITieredDeskVarianceRule
{
    List<string> Check(TieredDeskVariance item);
}

public sealed record TieredDeskVarianceAllowedNotesRule(List<string> Allowed) : ITieredDeskVarianceRule
{
    public List<string> Check(TieredDeskVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskVarianceUpdatedAtRangeRule(int Min, int Max) : ITieredDeskVarianceRule
{
    public List<string> Check(TieredDeskVariance item)
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

public sealed record TieredDeskVarianceFteRangeRule(double Min, double Max) : ITieredDeskVarianceRule
{
    public List<string> Check(TieredDeskVariance item)
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

public sealed record TieredDeskVarianceCodePrefixRule(string Prefix) : ITieredDeskVarianceRule
{
    public List<string> Check(TieredDeskVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskVarianceHoursRangeRule(double Min, double Max) : ITieredDeskVarianceRule
{
    public List<string> Check(TieredDeskVariance item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered desk variance.</summary>
public sealed record TieredDeskVarianceValidator(List<ITieredDeskVarianceRule> Rules)
{
    public List<string> Validate(TieredDeskVariance item)
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

    public void RequireValid(TieredDeskVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredDeskVariance> items)
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

/// <summary>Functions of tiered desk variance rules.</summary>
public static class TieredDeskVarianceRules
{
    public static TieredDeskVarianceValidator DefaultTieredDeskVarianceValidator()
    {
        return new TieredDeskVarianceValidator(
            new List<ITieredDeskVarianceRule>
            {
                new TieredDeskVarianceAllowedNotesRule(new List<string>()),
                new TieredDeskVarianceUpdatedAtRangeRule(16, 32),
                new TieredDeskVarianceFteRangeRule(7.7, 631.0),
                new TieredDeskVarianceCodePrefixRule("ACM"),
                new TieredDeskVarianceHoursRangeRule(9.7, 4458.1),
            });
    }
}
