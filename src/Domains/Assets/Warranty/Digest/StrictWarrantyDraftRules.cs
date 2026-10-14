namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict warranty draft must pass. Check returns its problems, or none.</summary>
public interface IStrictWarrantyDraftRule
{
    List<string> Check(StrictWarrantyDraft item);
}

public sealed record StrictWarrantyDraftCodePrefixRule(string Prefix) : IStrictWarrantyDraftRule
{
    public List<string> Check(StrictWarrantyDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyDraftSourceLengthRule(int MaxLength) : IStrictWarrantyDraftRule
{
    public List<string> Check(StrictWarrantyDraft item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyDraftAllowedCodeRule(List<string> Allowed) : IStrictWarrantyDraftRule
{
    public List<string> Check(StrictWarrantyDraft item)
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

public sealed record StrictWarrantyDraftLeaveDaysRangeRule(int Min, int Max) : IStrictWarrantyDraftRule
{
    public List<string> Check(StrictWarrantyDraft item)
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

public sealed record StrictWarrantyDraftVisibleGuardRule(int Limit) : IStrictWarrantyDraftRule
{
    public List<string> Check(StrictWarrantyDraft item)
    {
        if (item.Visible && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict warranty draft.</summary>
public sealed record StrictWarrantyDraftValidator(List<IStrictWarrantyDraftRule> Rules)
{
    public List<string> Validate(StrictWarrantyDraft item)
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

    public void RequireValid(StrictWarrantyDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictWarrantyDraft> items)
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

/// <summary>Functions of strict warranty draft rules.</summary>
public static class StrictWarrantyDraftRules
{
    public static StrictWarrantyDraftValidator DefaultStrictWarrantyDraftValidator()
    {
        return new StrictWarrantyDraftValidator(
            new List<IStrictWarrantyDraftRule>
            {
                new StrictWarrantyDraftCodePrefixRule("Q"),
                new StrictWarrantyDraftSourceLengthRule(120),
                new StrictWarrantyDraftAllowedCodeRule(new List<string>()),
                new StrictWarrantyDraftLeaveDaysRangeRule(32, 3600),
                new StrictWarrantyDraftVisibleGuardRule(3600),
            });
    }
}
