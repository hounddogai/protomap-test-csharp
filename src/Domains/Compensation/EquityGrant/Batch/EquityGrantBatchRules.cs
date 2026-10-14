namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant batch must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantBatchRule
{
    List<string> Check(EquityGrantBatch item);
}

public sealed record EquityGrantBatchAllowedSkillRule(List<string> Allowed) : IEquityGrantBatchRule
{
    public List<string> Check(EquityGrantBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBatchVisibleGuardRule(double Limit) : IEquityGrantBatchRule
{
    public List<string> Check(EquityGrantBatch item)
    {
        if (item.Visible && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBatchEmployeeIdLengthRule(int MaxLength) : IEquityGrantBatchRule
{
    public List<string> Check(EquityGrantBatch item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBatchHoursRangeRule(double Min, double Max) : IEquityGrantBatchRule
{
    public List<string> Check(EquityGrantBatch item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantBatchCodePrefixRule(string Prefix) : IEquityGrantBatchRule
{
    public List<string> Check(EquityGrantBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBatchWeightRangeRule(double Min, double Max) : IEquityGrantBatchRule
{
    public List<string> Check(EquityGrantBatch item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant batch.</summary>
public sealed record EquityGrantBatchValidator(List<IEquityGrantBatchRule> Rules)
{
    public List<string> Validate(EquityGrantBatch item)
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

    public void RequireValid(EquityGrantBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantBatch> items)
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

/// <summary>Functions of equity grant batch rules.</summary>
public static class EquityGrantBatchRules
{
    public static EquityGrantBatchValidator DefaultEquityGrantBatchValidator()
    {
        return new EquityGrantBatchValidator(
            new List<IEquityGrantBatchRule>
            {
                new EquityGrantBatchAllowedSkillRule(new List<string>()),
                new EquityGrantBatchVisibleGuardRule(2.3),
                new EquityGrantBatchEmployeeIdLengthRule(80),
                new EquityGrantBatchHoursRangeRule(8.4, 2465.4),
                new EquityGrantBatchCodePrefixRule("OPS"),
                new EquityGrantBatchWeightRangeRule(0.2, 2300.1),
            });
    }
}
