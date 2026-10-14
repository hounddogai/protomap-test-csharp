namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>A rule a rolling reorg summary must pass. Check returns its problems, or none.</summary>
public interface IRollingReorgSummaryRule
{
    List<string> Check(RollingReorgSummary item);
}

public sealed record RollingReorgSummaryRequiredExternalRefRule(string Message) : IRollingReorgSummaryRule
{
    public List<string> Check(RollingReorgSummary item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingReorgSummaryCodePrefixRule(string Prefix) : IRollingReorgSummaryRule
{
    public List<string> Check(RollingReorgSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingReorgSummaryCodeLengthRule(int MaxLength) : IRollingReorgSummaryRule
{
    public List<string> Check(RollingReorgSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingReorgSummaryRequiredCodeRule(string Message) : IRollingReorgSummaryRule
{
    public List<string> Check(RollingReorgSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingReorgSummaryVersionRangeRule(int Min, int Max) : IRollingReorgSummaryRule
{
    public List<string> Check(RollingReorgSummary item)
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

/// <summary>Runs every rule of a rolling reorg summary.</summary>
public sealed record RollingReorgSummaryValidator(List<IRollingReorgSummaryRule> Rules)
{
    public List<string> Validate(RollingReorgSummary item)
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

    public void RequireValid(RollingReorgSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingReorgSummary> items)
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

/// <summary>Functions of rolling reorg summary rules.</summary>
public static class RollingReorgSummaryRules
{
    public static RollingReorgSummaryValidator DefaultRollingReorgSummaryValidator()
    {
        return new RollingReorgSummaryValidator(
            new List<IRollingReorgSummaryRule>
            {
                new RollingReorgSummaryRequiredExternalRefRule("rolling reorg summary needs this field"),
                new RollingReorgSummaryCodePrefixRule("OPS"),
                new RollingReorgSummaryCodeLengthRule(86400),
                new RollingReorgSummaryRequiredCodeRule("rolling reorg summary needs an entry"),
                new RollingReorgSummaryVersionRangeRule(16, 3600),
            });
    }
}
