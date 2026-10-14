namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling retro pay assignment must pass. Check returns its problems, or none.</summary>
public interface IRollingRetroPayAssignmentRule
{
    List<string> Check(RollingRetroPayAssignment item);
}

public sealed record RollingRetroPayAssignmentDurationMinutesRangeRule(int Min, int Max) : IRollingRetroPayAssignmentRule
{
    public List<string> Check(RollingRetroPayAssignment item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPayAssignmentRevisionRangeRule(int Min, int Max) : IRollingRetroPayAssignmentRule
{
    public List<string> Check(RollingRetroPayAssignment item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPayAssignmentLocationLengthRule(int MaxLength) : IRollingRetroPayAssignmentRule
{
    public List<string> Check(RollingRetroPayAssignment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPayAssignmentAllowedTitleRule(List<string> Allowed) : IRollingRetroPayAssignmentRule
{
    public List<string> Check(RollingRetroPayAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPayAssignmentCodePrefixRule(string Prefix) : IRollingRetroPayAssignmentRule
{
    public List<string> Check(RollingRetroPayAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPayAssignmentRequiredLocationRule(string Message) : IRollingRetroPayAssignmentRule
{
    public List<string> Check(RollingRetroPayAssignment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling retro pay assignment.</summary>
public sealed record RollingRetroPayAssignmentValidator(List<IRollingRetroPayAssignmentRule> Rules)
{
    public List<string> Validate(RollingRetroPayAssignment item)
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

    public void RequireValid(RollingRetroPayAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRetroPayAssignment> items)
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

/// <summary>Functions of rolling retro pay assignment rules.</summary>
public static class RollingRetroPayAssignmentRules
{
    public static RollingRetroPayAssignmentValidator DefaultRollingRetroPayAssignmentValidator()
    {
        return new RollingRetroPayAssignmentValidator(
            new List<IRollingRetroPayAssignmentRule>
            {
                new RollingRetroPayAssignmentDurationMinutesRangeRule(3600, 3600),
                new RollingRetroPayAssignmentRevisionRangeRule(32, 64),
                new RollingRetroPayAssignmentLocationLengthRule(255),
                new RollingRetroPayAssignmentAllowedTitleRule(new List<string> { "global" }),
                new RollingRetroPayAssignmentCodePrefixRule("REF"),
                new RollingRetroPayAssignmentRequiredLocationRule("rolling retro pay assignment needs this field"),
            });
    }
}
