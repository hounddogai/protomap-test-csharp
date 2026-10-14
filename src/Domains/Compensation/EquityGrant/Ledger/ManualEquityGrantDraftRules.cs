namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual equity grant draft must pass. Check returns its problems, or none.</summary>
public interface IManualEquityGrantDraftRule
{
    List<string> Check(ManualEquityGrantDraft item);
}

public sealed record ManualEquityGrantDraftRequiredChecksumRule(string Message) : IManualEquityGrantDraftRule
{
    public List<string> Check(ManualEquityGrantDraft item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantDraftChecksumLengthRule(int MaxLength) : IManualEquityGrantDraftRule
{
    public List<string> Check(ManualEquityGrantDraft item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantDraftAllowedCodeRule(List<string> Allowed) : IManualEquityGrantDraftRule
{
    public List<string> Check(ManualEquityGrantDraft item)
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

public sealed record ManualEquityGrantDraftCodePrefixRule(string Prefix) : IManualEquityGrantDraftRule
{
    public List<string> Check(ManualEquityGrantDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantDraftVersionRangeRule(int Min, int Max) : IManualEquityGrantDraftRule
{
    public List<string> Check(ManualEquityGrantDraft item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualEquityGrantDraftRequiredTitleRule(string Message) : IManualEquityGrantDraftRule
{
    public List<string> Check(ManualEquityGrantDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual equity grant draft.</summary>
public sealed record ManualEquityGrantDraftValidator(List<IManualEquityGrantDraftRule> Rules)
{
    public List<string> Validate(ManualEquityGrantDraft item)
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

    public void RequireValid(ManualEquityGrantDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualEquityGrantDraft> items)
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

/// <summary>Functions of manual equity grant draft rules.</summary>
public static class ManualEquityGrantDraftRules
{
    public static ManualEquityGrantDraftValidator DefaultManualEquityGrantDraftValidator()
    {
        return new ManualEquityGrantDraftValidator(
            new List<IManualEquityGrantDraftRule>
            {
                new ManualEquityGrantDraftRequiredChecksumRule("manual equity grant draft needs an entry"),
                new ManualEquityGrantDraftChecksumLengthRule(255),
                new ManualEquityGrantDraftAllowedCodeRule(new List<string>()),
                new ManualEquityGrantDraftCodePrefixRule("REF"),
                new ManualEquityGrantDraftVersionRangeRule(32, 120),
                new ManualEquityGrantDraftRequiredTitleRule("manual equity grant draft needs this field"),
            });
    }
}
