namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant draft must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantDraftRule
{
    List<string> Check(EquityGrantDraft item);
}

public sealed record EquityGrantDraftCurrencyLengthRule(int MaxLength) : IEquityGrantDraftRule
{
    public List<string> Check(EquityGrantDraft item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantDraftRequiredCodeRule(string Message) : IEquityGrantDraftRule
{
    public List<string> Check(EquityGrantDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantDraftAllowedTitleRule(List<string> Allowed) : IEquityGrantDraftRule
{
    public List<string> Check(EquityGrantDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantDraftRequiredCurrencyRule(string Message) : IEquityGrantDraftRule
{
    public List<string> Check(EquityGrantDraft item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantDraftCodePrefixRule(string Prefix) : IEquityGrantDraftRule
{
    public List<string> Check(EquityGrantDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant draft.</summary>
public sealed record EquityGrantDraftValidator(List<IEquityGrantDraftRule> Rules)
{
    public List<string> Validate(EquityGrantDraft item)
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

    public void RequireValid(EquityGrantDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantDraft> items)
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

/// <summary>Functions of equity grant draft rules.</summary>
public static class EquityGrantDraftRules
{
    public static EquityGrantDraftValidator DefaultEquityGrantDraftValidator()
    {
        return new EquityGrantDraftValidator(
            new List<IEquityGrantDraftRule>
            {
                new EquityGrantDraftCurrencyLengthRule(1000),
                new EquityGrantDraftRequiredCodeRule("equity grant draft needs this field"),
                new EquityGrantDraftAllowedTitleRule(new List<string> { "batched", "lenient", "primary" }),
                new EquityGrantDraftRequiredCurrencyRule("equity grant draft needs an entry"),
                new EquityGrantDraftCodePrefixRule("Q"),
            });
    }
}
