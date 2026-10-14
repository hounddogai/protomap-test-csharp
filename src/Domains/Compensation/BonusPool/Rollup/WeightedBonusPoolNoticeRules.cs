namespace Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>A rule a weighted bonus pool notice must pass. Check returns its problems, or none.</summary>
public interface IWeightedBonusPoolNoticeRule
{
    List<string> Check(WeightedBonusPoolNotice item);
}

public sealed record WeightedBonusPoolNoticeLimitRangeRule(int Min, int Max) : IWeightedBonusPoolNoticeRule
{
    public List<string> Check(WeightedBonusPoolNotice item)
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

public sealed record WeightedBonusPoolNoticeMaximumRangeRule(double Min, double Max) : IWeightedBonusPoolNoticeRule
{
    public List<string> Check(WeightedBonusPoolNotice item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolNoticeCodePrefixRule(string Prefix) : IWeightedBonusPoolNoticeRule
{
    public List<string> Check(WeightedBonusPoolNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolNoticeCodeLengthRule(int MaxLength) : IWeightedBonusPoolNoticeRule
{
    public List<string> Check(WeightedBonusPoolNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolNoticeEmployeeIdLengthRule(int MaxLength) : IWeightedBonusPoolNoticeRule
{
    public List<string> Check(WeightedBonusPoolNotice item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolNoticeRequiredNameRule(string Message) : IWeightedBonusPoolNoticeRule
{
    public List<string> Check(WeightedBonusPoolNotice item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted bonus pool notice.</summary>
public sealed record WeightedBonusPoolNoticeValidator(List<IWeightedBonusPoolNoticeRule> Rules)
{
    public List<string> Validate(WeightedBonusPoolNotice item)
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

    public void RequireValid(WeightedBonusPoolNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBonusPoolNotice> items)
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

/// <summary>Functions of weighted bonus pool notice rules.</summary>
public static class WeightedBonusPoolNoticeRules
{
    public static WeightedBonusPoolNoticeValidator DefaultWeightedBonusPoolNoticeValidator()
    {
        return new WeightedBonusPoolNoticeValidator(
            new List<IWeightedBonusPoolNoticeRule>
            {
                new WeightedBonusPoolNoticeLimitRangeRule(32, 1000),
                new WeightedBonusPoolNoticeMaximumRangeRule(6.3, 1399.3),
                new WeightedBonusPoolNoticeCodePrefixRule("REF"),
                new WeightedBonusPoolNoticeCodeLengthRule(86400),
                new WeightedBonusPoolNoticeEmployeeIdLengthRule(255),
                new WeightedBonusPoolNoticeRequiredNameRule("weighted bonus pool notice needs a value"),
            });
    }
}
