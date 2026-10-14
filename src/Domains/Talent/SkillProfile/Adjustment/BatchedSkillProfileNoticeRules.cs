namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>A rule a batched skill profile notice must pass. Check returns its problems, or none.</summary>
public interface IBatchedSkillProfileNoticeRule
{
    List<string> Check(BatchedSkillProfileNotice item);
}

public sealed record BatchedSkillProfileNoticeMarginRangeRule(double Min, double Max) : IBatchedSkillProfileNoticeRule
{
    public List<string> Check(BatchedSkillProfileNotice item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedSkillProfileNoticeRequiredLabelRule(string Message) : IBatchedSkillProfileNoticeRule
{
    public List<string> Check(BatchedSkillProfileNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileNoticeCodeLengthRule(int MaxLength) : IBatchedSkillProfileNoticeRule
{
    public List<string> Check(BatchedSkillProfileNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileNoticeFloorValueRangeRule(double Min, double Max) : IBatchedSkillProfileNoticeRule
{
    public List<string> Check(BatchedSkillProfileNotice item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedSkillProfileNoticeCodePrefixRule(string Prefix) : IBatchedSkillProfileNoticeRule
{
    public List<string> Check(BatchedSkillProfileNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileNoticeRequiredSkillRule(string Message) : IBatchedSkillProfileNoticeRule
{
    public List<string> Check(BatchedSkillProfileNotice item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched skill profile notice.</summary>
public sealed record BatchedSkillProfileNoticeValidator(List<IBatchedSkillProfileNoticeRule> Rules)
{
    public List<string> Validate(BatchedSkillProfileNotice item)
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

    public void RequireValid(BatchedSkillProfileNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedSkillProfileNotice> items)
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

/// <summary>Functions of batched skill profile notice rules.</summary>
public static class BatchedSkillProfileNoticeRules
{
    public static BatchedSkillProfileNoticeValidator DefaultBatchedSkillProfileNoticeValidator()
    {
        return new BatchedSkillProfileNoticeValidator(
            new List<IBatchedSkillProfileNoticeRule>
            {
                new BatchedSkillProfileNoticeMarginRangeRule(9.3, 2673.2),
                new BatchedSkillProfileNoticeRequiredLabelRule("batched skill profile notice needs a value"),
                new BatchedSkillProfileNoticeCodeLengthRule(1000),
                new BatchedSkillProfileNoticeFloorValueRangeRule(7.8, 590.7),
                new BatchedSkillProfileNoticeCodePrefixRule("Q"),
                new BatchedSkillProfileNoticeRequiredSkillRule("batched skill profile notice needs an entry"),
            });
    }
}
