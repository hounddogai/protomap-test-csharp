namespace Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>A rule a desk policy must pass. Check returns its problems, or none.</summary>
public interface IDeskPolicyRule
{
    List<string> Check(DeskPolicy item);
}

public sealed record DeskPolicyWindowDaysRangeRule(int Min, int Max) : IDeskPolicyRule
{
    public List<string> Check(DeskPolicy item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskPolicyCodePrefixRule(string Prefix) : IDeskPolicyRule
{
    public List<string> Check(DeskPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskPolicyCodeLengthRule(int MaxLength) : IDeskPolicyRule
{
    public List<string> Check(DeskPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk policy.</summary>
public sealed record DeskPolicyValidator(List<IDeskPolicyRule> Rules)
{
    public List<string> Validate(DeskPolicy item)
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

    public void RequireValid(DeskPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskPolicy> items)
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

/// <summary>Functions of desk policy rules.</summary>
public static class DeskPolicyRules
{
    public static DeskPolicyValidator DefaultDeskPolicyValidator()
    {
        return new DeskPolicyValidator(
            new List<IDeskPolicyRule>
            {
                new DeskPolicyWindowDaysRangeRule(3600, 86400),
                new DeskPolicyCodePrefixRule("X"),
                new DeskPolicyCodeLengthRule(32),
            });
    }
}
