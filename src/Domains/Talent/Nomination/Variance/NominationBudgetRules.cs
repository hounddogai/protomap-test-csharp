namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination budget must pass. Check returns its problems, or none.</summary>
public interface INominationBudgetRule
{
    List<string> Check(NominationBudget item);
}

public sealed record NominationBudgetTitleLengthRule(int MaxLength) : INominationBudgetRule
{
    public List<string> Check(NominationBudget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationBudgetExternalRefLengthRule(int MaxLength) : INominationBudgetRule
{
    public List<string> Check(NominationBudget item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationBudgetCodePrefixRule(string Prefix) : INominationBudgetRule
{
    public List<string> Check(NominationBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationBudgetRequiredTitleRule(string Message) : INominationBudgetRule
{
    public List<string> Check(NominationBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationBudgetVersionRangeRule(int Min, int Max) : INominationBudgetRule
{
    public List<string> Check(NominationBudget item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationBudgetAllowedExternalRefRule(List<string> Allowed) : INominationBudgetRule
{
    public List<string> Check(NominationBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination budget.</summary>
public sealed record NominationBudgetValidator(List<INominationBudgetRule> Rules)
{
    public List<string> Validate(NominationBudget item)
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

    public void RequireValid(NominationBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationBudget> items)
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

/// <summary>Functions of nomination budget rules.</summary>
public static class NominationBudgetRules
{
    public static NominationBudgetValidator DefaultNominationBudgetValidator()
    {
        return new NominationBudgetValidator(
            new List<INominationBudgetRule>
            {
                new NominationBudgetTitleLengthRule(255),
                new NominationBudgetExternalRefLengthRule(3600),
                new NominationBudgetCodePrefixRule("X"),
                new NominationBudgetRequiredTitleRule("nomination budget needs this field"),
                new NominationBudgetVersionRangeRule(64, 255),
                new NominationBudgetAllowedExternalRefRule(new List<string> { "adaptive", "lenient", "scheduled" }),
            });
    }
}
