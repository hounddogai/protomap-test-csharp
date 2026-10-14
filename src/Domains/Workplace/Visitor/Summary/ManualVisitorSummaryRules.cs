namespace Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>A rule a manual visitor summary must pass. Check returns its problems, or none.</summary>
public interface IManualVisitorSummaryRule
{
    List<string> Check(ManualVisitorSummary item);
}

public sealed record ManualVisitorSummaryCodePrefixRule(string Prefix) : IManualVisitorSummaryRule
{
    public List<string> Check(ManualVisitorSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualVisitorSummaryArchivedGuardRule(int Limit) : IManualVisitorSummaryRule
{
    public List<string> Check(ManualVisitorSummary item)
    {
        if (item.Archived && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ManualVisitorSummaryFloorValueRangeRule(double Min, double Max) : IManualVisitorSummaryRule
{
    public List<string> Check(ManualVisitorSummary item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual visitor summary.</summary>
public sealed record ManualVisitorSummaryValidator(List<IManualVisitorSummaryRule> Rules)
{
    public List<string> Validate(ManualVisitorSummary item)
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

    public void RequireValid(ManualVisitorSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualVisitorSummary> items)
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

/// <summary>Functions of manual visitor summary rules.</summary>
public static class ManualVisitorSummaryRules
{
    public static ManualVisitorSummaryValidator DefaultManualVisitorSummaryValidator()
    {
        return new ManualVisitorSummaryValidator(
            new List<IManualVisitorSummaryRule>
            {
                new ManualVisitorSummaryCodePrefixRule("X"),
                new ManualVisitorSummaryArchivedGuardRule(1000),
                new ManualVisitorSummaryFloorValueRangeRule(7.8, 89.2),
            });
    }
}
