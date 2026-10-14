namespace Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>A rule a legacy retro pay snapshot must pass. Check returns its problems, or none.</summary>
public interface ILegacyRetroPaySnapshotRule
{
    List<string> Check(LegacyRetroPaySnapshot item);
}

public sealed record LegacyRetroPaySnapshotCodePrefixRule(string Prefix) : ILegacyRetroPaySnapshotRule
{
    public List<string> Check(LegacyRetroPaySnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySnapshotCreatedAtRangeRule(int Min, int Max) : ILegacyRetroPaySnapshotRule
{
    public List<string> Check(LegacyRetroPaySnapshot item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySnapshotCodeLengthRule(int MaxLength) : ILegacyRetroPaySnapshotRule
{
    public List<string> Check(LegacyRetroPaySnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySnapshotTitleLengthRule(int MaxLength) : ILegacyRetroPaySnapshotRule
{
    public List<string> Check(LegacyRetroPaySnapshot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySnapshotBadgeLengthRule(int MaxLength) : ILegacyRetroPaySnapshotRule
{
    public List<string> Check(LegacyRetroPaySnapshot item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySnapshotRequiredChecksumRule(string Message) : ILegacyRetroPaySnapshotRule
{
    public List<string> Check(LegacyRetroPaySnapshot item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy retro pay snapshot.</summary>
public sealed record LegacyRetroPaySnapshotValidator(List<ILegacyRetroPaySnapshotRule> Rules)
{
    public List<string> Validate(LegacyRetroPaySnapshot item)
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

    public void RequireValid(LegacyRetroPaySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRetroPaySnapshot> items)
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

/// <summary>Functions of legacy retro pay snapshot rules.</summary>
public static class LegacyRetroPaySnapshotRules
{
    public static LegacyRetroPaySnapshotValidator DefaultLegacyRetroPaySnapshotValidator()
    {
        return new LegacyRetroPaySnapshotValidator(
            new List<ILegacyRetroPaySnapshotRule>
            {
                new LegacyRetroPaySnapshotCodePrefixRule("OPS"),
                new LegacyRetroPaySnapshotCreatedAtRangeRule(64, 255),
                new LegacyRetroPaySnapshotCodeLengthRule(86400),
                new LegacyRetroPaySnapshotTitleLengthRule(64),
                new LegacyRetroPaySnapshotBadgeLengthRule(16),
                new LegacyRetroPaySnapshotRequiredChecksumRule("legacy retro pay snapshot needs this field"),
            });
    }
}
