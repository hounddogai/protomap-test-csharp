namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination rollup must pass. Check returns its problems, or none.</summary>
public interface INominationRollupRule
{
    List<string> Check(NominationRollup item);
}

public sealed record NominationRollupAllowedCodeRule(List<string> Allowed) : INominationRollupRule
{
    public List<string> Check(NominationRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record NominationRollupCodeLengthRule(int MaxLength) : INominationRollupRule
{
    public List<string> Check(NominationRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationRollupCodePrefixRule(string Prefix) : INominationRollupRule
{
    public List<string> Check(NominationRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationRollupLabelLengthRule(int MaxLength) : INominationRollupRule
{
    public List<string> Check(NominationRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination rollup.</summary>
public sealed record NominationRollupValidator(List<INominationRollupRule> Rules)
{
    public List<string> Validate(NominationRollup item)
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

    public void RequireValid(NominationRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationRollup> items)
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

/// <summary>Functions of nomination rollup rules.</summary>
public static class NominationRollupRules
{
    public static NominationRollupValidator DefaultNominationRollupValidator()
    {
        return new NominationRollupValidator(
            new List<INominationRollupRule>
            {
                new NominationRollupAllowedCodeRule(new List<string> { "lenient" }),
                new NominationRollupCodeLengthRule(3600),
                new NominationRollupCodePrefixRule("OPS"),
                new NominationRollupLabelLengthRule(255),
            });
    }
}
