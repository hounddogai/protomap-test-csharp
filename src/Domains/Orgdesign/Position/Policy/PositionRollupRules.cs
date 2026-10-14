namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position rollup must pass. Check returns its problems, or none.</summary>
public interface IPositionRollupRule
{
    List<string> Check(PositionRollup item);
}

public sealed record PositionRollupSkillLengthRule(int MaxLength) : IPositionRollupRule
{
    public List<string> Check(PositionRollup item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionRollupRequiredSourceRule(string Message) : IPositionRollupRule
{
    public List<string> Check(PositionRollup item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionRollupSourceLengthRule(int MaxLength) : IPositionRollupRule
{
    public List<string> Check(PositionRollup item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionRollupAllowedGradeRule(List<string> Allowed) : IPositionRollupRule
{
    public List<string> Check(PositionRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PositionRollupRequiredCodeRule(string Message) : IPositionRollupRule
{
    public List<string> Check(PositionRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionRollupRequiredTitleRule(string Message) : IPositionRollupRule
{
    public List<string> Check(PositionRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position rollup.</summary>
public sealed record PositionRollupValidator(List<IPositionRollupRule> Rules)
{
    public List<string> Validate(PositionRollup item)
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

    public void RequireValid(PositionRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionRollup> items)
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

/// <summary>Functions of position rollup rules.</summary>
public static class PositionRollupRules
{
    public static PositionRollupValidator DefaultPositionRollupValidator()
    {
        return new PositionRollupValidator(
            new List<IPositionRollupRule>
            {
                new PositionRollupSkillLengthRule(1000),
                new PositionRollupRequiredSourceRule("position rollup needs an entry"),
                new PositionRollupSourceLengthRule(32),
                new PositionRollupAllowedGradeRule(new List<string> { "weighted" }),
                new PositionRollupRequiredCodeRule("position rollup needs a value"),
                new PositionRollupRequiredTitleRule("position rollup needs this field"),
            });
    }
}
