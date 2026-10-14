namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk exception must pass. Check returns its problems, or none.</summary>
public interface IDeskExceptionRule
{
    List<string> Check(DeskException item);
}

public sealed record DeskExceptionCodeLengthRule(int MaxLength) : IDeskExceptionRule
{
    public List<string> Check(DeskException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskExceptionPinnedGuardRule(int Limit) : IDeskExceptionRule
{
    public List<string> Check(DeskException item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DeskExceptionAllowedTitleRule(List<string> Allowed) : IDeskExceptionRule
{
    public List<string> Check(DeskException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record DeskExceptionCodePrefixRule(string Prefix) : IDeskExceptionRule
{
    public List<string> Check(DeskException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskExceptionLeaveDaysRangeRule(int Min, int Max) : IDeskExceptionRule
{
    public List<string> Check(DeskException item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a desk exception.</summary>
public sealed record DeskExceptionValidator(List<IDeskExceptionRule> Rules)
{
    public List<string> Validate(DeskException item)
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

    public void RequireValid(DeskException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskException> items)
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

/// <summary>Functions of desk exception rules.</summary>
public static class DeskExceptionRules
{
    public static DeskExceptionValidator DefaultDeskExceptionValidator()
    {
        return new DeskExceptionValidator(
            new List<IDeskExceptionRule>
            {
                new DeskExceptionCodeLengthRule(120),
                new DeskExceptionPinnedGuardRule(255),
                new DeskExceptionAllowedTitleRule(new List<string> { "strict", "strict", "adaptive" }),
                new DeskExceptionCodePrefixRule("ACM"),
                new DeskExceptionLeaveDaysRangeRule(16, 16),
            });
    }
}
