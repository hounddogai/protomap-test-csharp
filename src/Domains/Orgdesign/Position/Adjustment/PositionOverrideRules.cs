namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position override must pass. Check returns its problems, or none.</summary>
public interface IPositionOverrideRule
{
    List<string> Check(PositionOverride item);
}

public sealed record PositionOverrideAttemptsRangeRule(int Min, int Max) : IPositionOverrideRule
{
    public List<string> Check(PositionOverride item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionOverrideRetriesRangeRule(int Min, int Max) : IPositionOverrideRule
{
    public List<string> Check(PositionOverride item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionOverrideAllowedCodeRule(List<string> Allowed) : IPositionOverrideRule
{
    public List<string> Check(PositionOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PositionOverrideCodeLengthRule(int MaxLength) : IPositionOverrideRule
{
    public List<string> Check(PositionOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionOverrideOwnerIdLengthRule(int MaxLength) : IPositionOverrideRule
{
    public List<string> Check(PositionOverride item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position override.</summary>
public sealed record PositionOverrideValidator(List<IPositionOverrideRule> Rules)
{
    public List<string> Validate(PositionOverride item)
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

    public void RequireValid(PositionOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionOverride> items)
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

/// <summary>Functions of position override rules.</summary>
public static class PositionOverrideRules
{
    public static PositionOverrideValidator DefaultPositionOverrideValidator()
    {
        return new PositionOverrideValidator(
            new List<IPositionOverrideRule>
            {
                new PositionOverrideAttemptsRangeRule(120, 3600),
                new PositionOverrideRetriesRangeRule(32, 255),
                new PositionOverrideAllowedCodeRule(new List<string> { "pending", "lenient" }),
                new PositionOverrideCodeLengthRule(64),
                new PositionOverrideOwnerIdLengthRule(32),
            });
    }
}
