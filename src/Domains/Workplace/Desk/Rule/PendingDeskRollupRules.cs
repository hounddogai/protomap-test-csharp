namespace Acme.Workforce.Workplace.Desk.Rule;

/// <summary>A rule a pending desk rollup must pass. Check returns its problems, or none.</summary>
public interface IPendingDeskRollupRule
{
    List<string> Check(PendingDeskRollup item);
}

public sealed record PendingDeskRollupCodeLengthRule(int MaxLength) : IPendingDeskRollupRule
{
    public List<string> Check(PendingDeskRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskRollupCodePrefixRule(string Prefix) : IPendingDeskRollupRule
{
    public List<string> Check(PendingDeskRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskRollupChecksumLengthRule(int MaxLength) : IPendingDeskRollupRule
{
    public List<string> Check(PendingDeskRollup item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskRollupRequiredCodeRule(string Message) : IPendingDeskRollupRule
{
    public List<string> Check(PendingDeskRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskRollupExpiresAtRangeRule(int Min, int Max) : IPendingDeskRollupRule
{
    public List<string> Check(PendingDeskRollup item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending desk rollup.</summary>
public sealed record PendingDeskRollupValidator(List<IPendingDeskRollupRule> Rules)
{
    public List<string> Validate(PendingDeskRollup item)
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

    public void RequireValid(PendingDeskRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingDeskRollup> items)
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

/// <summary>Functions of pending desk rollup rules.</summary>
public static class PendingDeskRollupRules
{
    public static PendingDeskRollupValidator DefaultPendingDeskRollupValidator()
    {
        return new PendingDeskRollupValidator(
            new List<IPendingDeskRollupRule>
            {
                new PendingDeskRollupCodeLengthRule(120),
                new PendingDeskRollupCodePrefixRule("OPS"),
                new PendingDeskRollupChecksumLengthRule(120),
                new PendingDeskRollupRequiredCodeRule("pending desk rollup needs a value"),
                new PendingDeskRollupExpiresAtRangeRule(32, 3600),
            });
    }
}
