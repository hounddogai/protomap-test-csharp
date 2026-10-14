namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary visitor batch must pass. Check returns its problems, or none.</summary>
public interface IPrimaryVisitorBatchRule
{
    List<string> Check(PrimaryVisitorBatch item);
}

public sealed record PrimaryVisitorBatchRequiredCodeRule(string Message) : IPrimaryVisitorBatchRule
{
    public List<string> Check(PrimaryVisitorBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorBatchCodePrefixRule(string Prefix) : IPrimaryVisitorBatchRule
{
    public List<string> Check(PrimaryVisitorBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorBatchSequenceRangeRule(int Min, int Max) : IPrimaryVisitorBatchRule
{
    public List<string> Check(PrimaryVisitorBatch item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorBatchLimitRangeRule(int Min, int Max) : IPrimaryVisitorBatchRule
{
    public List<string> Check(PrimaryVisitorBatch item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryVisitorBatchAllowedCodeRule(List<string> Allowed) : IPrimaryVisitorBatchRule
{
    public List<string> Check(PrimaryVisitorBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary visitor batch.</summary>
public sealed record PrimaryVisitorBatchValidator(List<IPrimaryVisitorBatchRule> Rules)
{
    public List<string> Validate(PrimaryVisitorBatch item)
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

    public void RequireValid(PrimaryVisitorBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryVisitorBatch> items)
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

/// <summary>Functions of primary visitor batch rules.</summary>
public static class PrimaryVisitorBatchRules
{
    public static PrimaryVisitorBatchValidator DefaultPrimaryVisitorBatchValidator()
    {
        return new PrimaryVisitorBatchValidator(
            new List<IPrimaryVisitorBatchRule>
            {
                new PrimaryVisitorBatchRequiredCodeRule("primary visitor batch needs this field"),
                new PrimaryVisitorBatchCodePrefixRule("REF"),
                new PrimaryVisitorBatchSequenceRangeRule(32, 3600),
                new PrimaryVisitorBatchLimitRangeRule(32, 120),
                new PrimaryVisitorBatchAllowedCodeRule(new List<string> { "lenient", "tiered" }),
            });
    }
}
