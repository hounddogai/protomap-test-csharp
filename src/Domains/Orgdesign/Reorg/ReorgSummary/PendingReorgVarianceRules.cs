namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending reorg variance must pass. Check returns its problems, or none.</summary>
public interface IPendingReorgVarianceRule
{
    List<string> Check(PendingReorgVariance item);
}

public sealed record PendingReorgVarianceQuantityRangeRule(int Min, int Max) : IPendingReorgVarianceRule
{
    public List<string> Check(PendingReorgVariance item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingReorgVarianceRequiredDescriptionRule(string Message) : IPendingReorgVarianceRule
{
    public List<string> Check(PendingReorgVariance item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgVarianceCodePrefixRule(string Prefix) : IPendingReorgVarianceRule
{
    public List<string> Check(PendingReorgVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgVarianceRequiredCodeRule(string Message) : IPendingReorgVarianceRule
{
    public List<string> Check(PendingReorgVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgVarianceAllowedCodeRule(List<string> Allowed) : IPendingReorgVarianceRule
{
    public List<string> Check(PendingReorgVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgVarianceApprovedGuardRule(int Limit) : IPendingReorgVarianceRule
{
    public List<string> Check(PendingReorgVariance item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending reorg variance.</summary>
public sealed record PendingReorgVarianceValidator(List<IPendingReorgVarianceRule> Rules)
{
    public List<string> Validate(PendingReorgVariance item)
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

    public void RequireValid(PendingReorgVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingReorgVariance> items)
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

/// <summary>Functions of pending reorg variance rules.</summary>
public static class PendingReorgVarianceRules
{
    public static PendingReorgVarianceValidator DefaultPendingReorgVarianceValidator()
    {
        return new PendingReorgVarianceValidator(
            new List<IPendingReorgVarianceRule>
            {
                new PendingReorgVarianceQuantityRangeRule(80, 1000),
                new PendingReorgVarianceRequiredDescriptionRule("pending reorg variance needs an entry"),
                new PendingReorgVarianceCodePrefixRule("REF"),
                new PendingReorgVarianceRequiredCodeRule("pending reorg variance needs a value"),
                new PendingReorgVarianceAllowedCodeRule(new List<string> { "lenient", "rolling", "fallback" }),
                new PendingReorgVarianceApprovedGuardRule(32),
            });
    }
}
