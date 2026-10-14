namespace Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>A rule a manual bonus pool draft must pass. Check returns its problems, or none.</summary>
public interface IManualBonusPoolDraftRule
{
    List<string> Check(ManualBonusPoolDraft item);
}

public sealed record ManualBonusPoolDraftChannelLengthRule(int MaxLength) : IManualBonusPoolDraftRule
{
    public List<string> Check(ManualBonusPoolDraft item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolDraftSummaryLengthRule(int MaxLength) : IManualBonusPoolDraftRule
{
    public List<string> Check(ManualBonusPoolDraft item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolDraftCodePrefixRule(string Prefix) : IManualBonusPoolDraftRule
{
    public List<string> Check(ManualBonusPoolDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolDraftRequiredCodeRule(string Message) : IManualBonusPoolDraftRule
{
    public List<string> Check(ManualBonusPoolDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolDraftRequiredNameRule(string Message) : IManualBonusPoolDraftRule
{
    public List<string> Check(ManualBonusPoolDraft item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual bonus pool draft.</summary>
public sealed record ManualBonusPoolDraftValidator(List<IManualBonusPoolDraftRule> Rules)
{
    public List<string> Validate(ManualBonusPoolDraft item)
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

    public void RequireValid(ManualBonusPoolDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBonusPoolDraft> items)
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

/// <summary>Functions of manual bonus pool draft rules.</summary>
public static class ManualBonusPoolDraftRules
{
    public static ManualBonusPoolDraftValidator DefaultManualBonusPoolDraftValidator()
    {
        return new ManualBonusPoolDraftValidator(
            new List<IManualBonusPoolDraftRule>
            {
                new ManualBonusPoolDraftChannelLengthRule(255),
                new ManualBonusPoolDraftSummaryLengthRule(64),
                new ManualBonusPoolDraftCodePrefixRule("Q"),
                new ManualBonusPoolDraftRequiredCodeRule("manual bonus pool draft needs an entry"),
                new ManualBonusPoolDraftRequiredNameRule("manual bonus pool draft needs an entry"),
            });
    }
}
