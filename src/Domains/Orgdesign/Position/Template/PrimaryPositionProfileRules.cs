namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary position profile must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPositionProfileRule
{
    List<string> Check(PrimaryPositionProfile item);
}

public sealed record PrimaryPositionProfileCodePrefixRule(string Prefix) : IPrimaryPositionProfileRule
{
    public List<string> Check(PrimaryPositionProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionProfileTitleLengthRule(int MaxLength) : IPrimaryPositionProfileRule
{
    public List<string> Check(PrimaryPositionProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionProfilePriorityRangeRule(int Min, int Max) : IPrimaryPositionProfileRule
{
    public List<string> Check(PrimaryPositionProfile item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionProfileMinimumRangeRule(double Min, double Max) : IPrimaryPositionProfileRule
{
    public List<string> Check(PrimaryPositionProfile item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryPositionProfileAllowedManagerIdRule(List<string> Allowed) : IPrimaryPositionProfileRule
{
    public List<string> Check(PrimaryPositionProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary position profile.</summary>
public sealed record PrimaryPositionProfileValidator(List<IPrimaryPositionProfileRule> Rules)
{
    public List<string> Validate(PrimaryPositionProfile item)
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

    public void RequireValid(PrimaryPositionProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPositionProfile> items)
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

/// <summary>Functions of primary position profile rules.</summary>
public static class PrimaryPositionProfileRules
{
    public static PrimaryPositionProfileValidator DefaultPrimaryPositionProfileValidator()
    {
        return new PrimaryPositionProfileValidator(
            new List<IPrimaryPositionProfileRule>
            {
                new PrimaryPositionProfileCodePrefixRule("X"),
                new PrimaryPositionProfileTitleLengthRule(64),
                new PrimaryPositionProfilePriorityRangeRule(16, 16),
                new PrimaryPositionProfileMinimumRangeRule(0.2, 1713.4),
                new PrimaryPositionProfileAllowedManagerIdRule(new List<string> { "default", "seasonal", "batched" }),
            });
    }
}
