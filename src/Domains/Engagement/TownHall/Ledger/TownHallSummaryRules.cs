namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall summary must pass. Check returns its problems, or none.</summary>
public interface ITownHallSummaryRule
{
    List<string> Check(TownHallSummary item);
}

public sealed record TownHallSummaryRequiredCodeRule(string Message) : ITownHallSummaryRule
{
    public List<string> Check(TownHallSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallSummaryAllowedLabelRule(List<string> Allowed) : ITownHallSummaryRule
{
    public List<string> Check(TownHallSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TownHallSummaryCodePrefixRule(string Prefix) : ITownHallSummaryRule
{
    public List<string> Check(TownHallSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall summary.</summary>
public sealed record TownHallSummaryValidator(List<ITownHallSummaryRule> Rules)
{
    public List<string> Validate(TownHallSummary item)
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

    public void RequireValid(TownHallSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallSummary> items)
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

/// <summary>Functions of town hall summary rules.</summary>
public static class TownHallSummaryRules
{
    public static TownHallSummaryValidator DefaultTownHallSummaryValidator()
    {
        return new TownHallSummaryValidator(
            new List<ITownHallSummaryRule>
            {
                new TownHallSummaryRequiredCodeRule("town hall summary needs an entry"),
                new TownHallSummaryAllowedLabelRule(new List<string>()),
                new TownHallSummaryCodePrefixRule("OPS"),
            });
    }
}
