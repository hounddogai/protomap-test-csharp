namespace Acme.Workforce.Assets.Assignment.Allocation;

/// <summary>A rule a assignment digest must pass. Check returns its problems, or none.</summary>
public interface IAssignmentDigestRule
{
    List<string> Check(AssignmentDigest item);
}

public sealed record AssignmentDigestDurationMinutesRangeRule(int Min, int Max) : IAssignmentDigestRule
{
    public List<string> Check(AssignmentDigest item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDigestLabelLengthRule(int MaxLength) : IAssignmentDigestRule
{
    public List<string> Check(AssignmentDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDigestFteRangeRule(double Min, double Max) : IAssignmentDigestRule
{
    public List<string> Check(AssignmentDigest item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDigestApprovedGuardRule(int Limit) : IAssignmentDigestRule
{
    public List<string> Check(AssignmentDigest item)
    {
        if (item.Approved && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDigestRequiredCountryRule(string Message) : IAssignmentDigestRule
{
    public List<string> Check(AssignmentDigest item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDigestAttemptsRangeRule(int Min, int Max) : IAssignmentDigestRule
{
    public List<string> Check(AssignmentDigest item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment digest.</summary>
public sealed record AssignmentDigestValidator(List<IAssignmentDigestRule> Rules)
{
    public List<string> Validate(AssignmentDigest item)
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

    public void RequireValid(AssignmentDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentDigest> items)
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

/// <summary>Functions of assignment digest rules.</summary>
public static class AssignmentDigestRules
{
    public static AssignmentDigestValidator DefaultAssignmentDigestValidator()
    {
        return new AssignmentDigestValidator(
            new List<IAssignmentDigestRule>
            {
                new AssignmentDigestDurationMinutesRangeRule(255, 255),
                new AssignmentDigestLabelLengthRule(86400),
                new AssignmentDigestFteRangeRule(8.6, 4880.6),
                new AssignmentDigestApprovedGuardRule(1000),
                new AssignmentDigestRequiredCountryRule("assignment digest needs an entry"),
                new AssignmentDigestAttemptsRangeRule(80, 1000),
            });
    }
}
