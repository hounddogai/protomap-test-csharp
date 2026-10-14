namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled equity grant variance must pass. Check returns its problems, or none.</summary>
public interface IScheduledEquityGrantVarianceRule
{
    List<string> Check(ScheduledEquityGrantVariance item);
}

public sealed record ScheduledEquityGrantVarianceVisibleGuardRule(double Limit) : IScheduledEquityGrantVarianceRule
{
    public List<string> Check(ScheduledEquityGrantVariance item)
    {
        if (item.Visible && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantVarianceNameLengthRule(int MaxLength) : IScheduledEquityGrantVarianceRule
{
    public List<string> Check(ScheduledEquityGrantVariance item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantVarianceNotesLengthRule(int MaxLength) : IScheduledEquityGrantVarianceRule
{
    public List<string> Check(ScheduledEquityGrantVariance item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantVarianceRequiredNotesRule(string Message) : IScheduledEquityGrantVarianceRule
{
    public List<string> Check(ScheduledEquityGrantVariance item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantVarianceRequiredNameRule(string Message) : IScheduledEquityGrantVarianceRule
{
    public List<string> Check(ScheduledEquityGrantVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantVarianceAllowedCodeRule(List<string> Allowed) : IScheduledEquityGrantVarianceRule
{
    public List<string> Check(ScheduledEquityGrantVariance item)
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

/// <summary>Runs every rule of a scheduled equity grant variance.</summary>
public sealed record ScheduledEquityGrantVarianceValidator(List<IScheduledEquityGrantVarianceRule> Rules)
{
    public List<string> Validate(ScheduledEquityGrantVariance item)
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

    public void RequireValid(ScheduledEquityGrantVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEquityGrantVariance> items)
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

/// <summary>Functions of scheduled equity grant variance rules.</summary>
public static class ScheduledEquityGrantVarianceRules
{
    public static ScheduledEquityGrantVarianceValidator DefaultScheduledEquityGrantVarianceValidator()
    {
        return new ScheduledEquityGrantVarianceValidator(
            new List<IScheduledEquityGrantVarianceRule>
            {
                new ScheduledEquityGrantVarianceVisibleGuardRule(8.3),
                new ScheduledEquityGrantVarianceNameLengthRule(255),
                new ScheduledEquityGrantVarianceNotesLengthRule(3600),
                new ScheduledEquityGrantVarianceRequiredNotesRule("scheduled equity grant variance needs an entry"),
                new ScheduledEquityGrantVarianceRequiredNameRule("scheduled equity grant variance needs this field"),
                new ScheduledEquityGrantVarianceAllowedCodeRule(new List<string>()),
            });
    }
}
