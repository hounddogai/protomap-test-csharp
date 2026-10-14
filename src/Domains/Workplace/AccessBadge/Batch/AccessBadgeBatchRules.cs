namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge batch must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeBatchRule
{
    List<string> Check(AccessBadgeBatch item);
}

public sealed record AccessBadgeBatchChannelLengthRule(int MaxLength) : IAccessBadgeBatchRule
{
    public List<string> Check(AccessBadgeBatch item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeBatchSalaryBandLengthRule(int MaxLength) : IAccessBadgeBatchRule
{
    public List<string> Check(AccessBadgeBatch item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeBatchNameLengthRule(int MaxLength) : IAccessBadgeBatchRule
{
    public List<string> Check(AccessBadgeBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeBatchAllowedBadgeRule(List<string> Allowed) : IAccessBadgeBatchRule
{
    public List<string> Check(AccessBadgeBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeBatchCodeLengthRule(int MaxLength) : IAccessBadgeBatchRule
{
    public List<string> Check(AccessBadgeBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeBatchRateRangeRule(double Min, double Max) : IAccessBadgeBatchRule
{
    public List<string> Check(AccessBadgeBatch item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge batch.</summary>
public sealed record AccessBadgeBatchValidator(List<IAccessBadgeBatchRule> Rules)
{
    public List<string> Validate(AccessBadgeBatch item)
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

    public void RequireValid(AccessBadgeBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeBatch> items)
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

/// <summary>Functions of access badge batch rules.</summary>
public static class AccessBadgeBatchRules
{
    public static AccessBadgeBatchValidator DefaultAccessBadgeBatchValidator()
    {
        return new AccessBadgeBatchValidator(
            new List<IAccessBadgeBatchRule>
            {
                new AccessBadgeBatchChannelLengthRule(64),
                new AccessBadgeBatchSalaryBandLengthRule(86400),
                new AccessBadgeBatchNameLengthRule(120),
                new AccessBadgeBatchAllowedBadgeRule(new List<string>()),
                new AccessBadgeBatchCodeLengthRule(64),
                new AccessBadgeBatchRateRangeRule(6.2, 779.0),
            });
    }
}
