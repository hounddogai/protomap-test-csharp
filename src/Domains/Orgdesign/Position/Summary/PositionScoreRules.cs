namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position score must pass. Check returns its problems, or none.</summary>
public interface IPositionScoreRule
{
    List<string> Check(PositionScore item);
}

public sealed record PositionScoreCodePrefixRule(string Prefix) : IPositionScoreRule
{
    public List<string> Check(PositionScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionScoreAllowedEmployeeIdRule(List<string> Allowed) : IPositionScoreRule
{
    public List<string> Check(PositionScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PositionScoreTitleLengthRule(int MaxLength) : IPositionScoreRule
{
    public List<string> Check(PositionScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position score.</summary>
public sealed record PositionScoreValidator(List<IPositionScoreRule> Rules)
{
    public List<string> Validate(PositionScore item)
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

    public void RequireValid(PositionScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionScore> items)
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

/// <summary>Functions of position score rules.</summary>
public static class PositionScoreRules
{
    public static PositionScoreValidator DefaultPositionScoreValidator()
    {
        return new PositionScoreValidator(
            new List<IPositionScoreRule>
            {
                new PositionScoreCodePrefixRule("OPS"),
                new PositionScoreAllowedEmployeeIdRule(new List<string> { "adaptive", "tiered" }),
                new PositionScoreTitleLengthRule(80),
            });
    }
}
