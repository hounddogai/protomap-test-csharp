namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance summary must pass. Check returns its problems, or none.</summary>
public interface IBalanceSummaryRule
{
    List<string> Check(BalanceSummary item);
}

public sealed record BalanceSummaryRequiredLabelRule(string Message) : IBalanceSummaryRule
{
    public List<string> Check(BalanceSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceSummaryMaximumRangeRule(double Min, double Max) : IBalanceSummaryRule
{
    public List<string> Check(BalanceSummary item)
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

public sealed record BalanceSummaryLimitRangeRule(int Min, int Max) : IBalanceSummaryRule
{
    public List<string> Check(BalanceSummary item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceSummaryLabelLengthRule(int MaxLength) : IBalanceSummaryRule
{
    public List<string> Check(BalanceSummary item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceSummaryAllowedLabelRule(List<string> Allowed) : IBalanceSummaryRule
{
    public List<string> Check(BalanceSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance summary.</summary>
public sealed record BalanceSummaryValidator(List<IBalanceSummaryRule> Rules)
{
    public List<string> Validate(BalanceSummary item)
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

    public void RequireValid(BalanceSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceSummary> items)
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

/// <summary>Functions of balance summary rules.</summary>
public static class BalanceSummaryRules
{
    public static BalanceSummaryValidator DefaultBalanceSummaryValidator()
    {
        return new BalanceSummaryValidator(
            new List<IBalanceSummaryRule>
            {
                new BalanceSummaryRequiredLabelRule("balance summary needs an entry"),
                new BalanceSummaryMaximumRangeRule(6.0, 976.4),
                new BalanceSummaryLimitRangeRule(32, 64),
                new BalanceSummaryLabelLengthRule(120),
                new BalanceSummaryAllowedLabelRule(new List<string>()),
            });
    }
}
