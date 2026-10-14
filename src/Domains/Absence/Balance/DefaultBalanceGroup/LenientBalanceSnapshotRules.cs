namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient balance snapshot must pass. Check returns its problems, or none.</summary>
public interface ILenientBalanceSnapshotRule
{
    List<string> Check(LenientBalanceSnapshot item);
}

public sealed record LenientBalanceSnapshotAllowedExternalRefRule(List<string> Allowed) : ILenientBalanceSnapshotRule
{
    public List<string> Check(LenientBalanceSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceSnapshotTenureMonthsRangeRule(int Min, int Max) : ILenientBalanceSnapshotRule
{
    public List<string> Check(LenientBalanceSnapshot item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientBalanceSnapshotRequiredTitleRule(string Message) : ILenientBalanceSnapshotRule
{
    public List<string> Check(LenientBalanceSnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceSnapshotNameLengthRule(int MaxLength) : ILenientBalanceSnapshotRule
{
    public List<string> Check(LenientBalanceSnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceSnapshotCodePrefixRule(string Prefix) : ILenientBalanceSnapshotRule
{
    public List<string> Check(LenientBalanceSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceSnapshotRequiredSourceRule(string Message) : ILenientBalanceSnapshotRule
{
    public List<string> Check(LenientBalanceSnapshot item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient balance snapshot.</summary>
public sealed record LenientBalanceSnapshotValidator(List<ILenientBalanceSnapshotRule> Rules)
{
    public List<string> Validate(LenientBalanceSnapshot item)
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

    public void RequireValid(LenientBalanceSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBalanceSnapshot> items)
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

/// <summary>Functions of lenient balance snapshot rules.</summary>
public static class LenientBalanceSnapshotRules
{
    public static LenientBalanceSnapshotValidator DefaultLenientBalanceSnapshotValidator()
    {
        return new LenientBalanceSnapshotValidator(
            new List<ILenientBalanceSnapshotRule>
            {
                new LenientBalanceSnapshotAllowedExternalRefRule(new List<string>()),
                new LenientBalanceSnapshotTenureMonthsRangeRule(64, 1000),
                new LenientBalanceSnapshotRequiredTitleRule("lenient balance snapshot needs this field"),
                new LenientBalanceSnapshotNameLengthRule(86400),
                new LenientBalanceSnapshotCodePrefixRule("REF"),
                new LenientBalanceSnapshotRequiredSourceRule("lenient balance snapshot needs a value"),
            });
    }
}
