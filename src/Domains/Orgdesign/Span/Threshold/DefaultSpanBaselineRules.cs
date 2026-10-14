namespace Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>A rule a default span baseline must pass. Check returns its problems, or none.</summary>
public interface IDefaultSpanBaselineRule
{
    List<string> Check(DefaultSpanBaseline item);
}

public sealed record DefaultSpanBaselineCodeLengthRule(int MaxLength) : IDefaultSpanBaselineRule
{
    public List<string> Check(DefaultSpanBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanBaselineLabelLengthRule(int MaxLength) : IDefaultSpanBaselineRule
{
    public List<string> Check(DefaultSpanBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanBaselineArchivedGuardRule(double Limit) : IDefaultSpanBaselineRule
{
    public List<string> Check(DefaultSpanBaseline item)
    {
        if (item.Archived && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanBaselineRequiredSummaryRule(string Message) : IDefaultSpanBaselineRule
{
    public List<string> Check(DefaultSpanBaseline item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default span baseline.</summary>
public sealed record DefaultSpanBaselineValidator(List<IDefaultSpanBaselineRule> Rules)
{
    public List<string> Validate(DefaultSpanBaseline item)
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

    public void RequireValid(DefaultSpanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSpanBaseline> items)
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

/// <summary>Functions of default span baseline rules.</summary>
public static class DefaultSpanBaselineRules
{
    public static DefaultSpanBaselineValidator DefaultDefaultSpanBaselineValidator()
    {
        return new DefaultSpanBaselineValidator(
            new List<IDefaultSpanBaselineRule>
            {
                new DefaultSpanBaselineCodeLengthRule(86400),
                new DefaultSpanBaselineLabelLengthRule(64),
                new DefaultSpanBaselineArchivedGuardRule(6.9),
                new DefaultSpanBaselineRequiredSummaryRule("default span baseline needs this field"),
            });
    }
}
