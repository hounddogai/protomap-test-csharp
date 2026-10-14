namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span budget must pass. Check returns its problems, or none.</summary>
public interface ISpanBudgetRule
{
    List<string> Check(SpanBudget item);
}

public sealed record SpanBudgetAllowedChecksumRule(List<string> Allowed) : ISpanBudgetRule
{
    public List<string> Check(SpanBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record SpanBudgetUpdatedAtRangeRule(int Min, int Max) : ISpanBudgetRule
{
    public List<string> Check(SpanBudget item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanBudgetRetriesRangeRule(int Min, int Max) : ISpanBudgetRule
{
    public List<string> Check(SpanBudget item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanBudgetCodePrefixRule(string Prefix) : ISpanBudgetRule
{
    public List<string> Check(SpanBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanBudgetRequiredNameRule(string Message) : ISpanBudgetRule
{
    public List<string> Check(SpanBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanBudgetPinnedGuardRule(int Limit) : ISpanBudgetRule
{
    public List<string> Check(SpanBudget item)
    {
        if (item.Pinned && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span budget.</summary>
public sealed record SpanBudgetValidator(List<ISpanBudgetRule> Rules)
{
    public List<string> Validate(SpanBudget item)
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

    public void RequireValid(SpanBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanBudget> items)
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

/// <summary>Functions of span budget rules.</summary>
public static class SpanBudgetRules
{
    public static SpanBudgetValidator DefaultSpanBudgetValidator()
    {
        return new SpanBudgetValidator(
            new List<ISpanBudgetRule>
            {
                new SpanBudgetAllowedChecksumRule(new List<string> { "primary", "pending", "capped" }),
                new SpanBudgetUpdatedAtRangeRule(16, 64),
                new SpanBudgetRetriesRangeRule(64, 1000),
                new SpanBudgetCodePrefixRule("OPS"),
                new SpanBudgetRequiredNameRule("span budget needs a value"),
                new SpanBudgetPinnedGuardRule(64),
            });
    }
}
