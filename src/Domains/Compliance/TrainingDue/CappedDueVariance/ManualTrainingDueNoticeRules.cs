namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>A rule a manual training due notice must pass. Check returns its problems, or none.</summary>
public interface IManualTrainingDueNoticeRule
{
    List<string> Check(ManualTrainingDueNotice item);
}

public sealed record ManualTrainingDueNoticeEmployeeIdLengthRule(int MaxLength) : IManualTrainingDueNoticeRule
{
    public List<string> Check(ManualTrainingDueNotice item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTrainingDueNoticeCodePrefixRule(string Prefix) : IManualTrainingDueNoticeRule
{
    public List<string> Check(ManualTrainingDueNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTrainingDueNoticeCodeLengthRule(int MaxLength) : IManualTrainingDueNoticeRule
{
    public List<string> Check(ManualTrainingDueNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTrainingDueNoticeNameLengthRule(int MaxLength) : IManualTrainingDueNoticeRule
{
    public List<string> Check(ManualTrainingDueNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTrainingDueNoticeCapacityRangeRule(int Min, int Max) : IManualTrainingDueNoticeRule
{
    public List<string> Check(ManualTrainingDueNotice item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTrainingDueNoticeRequiredCodeRule(string Message) : IManualTrainingDueNoticeRule
{
    public List<string> Check(ManualTrainingDueNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual training due notice.</summary>
public sealed record ManualTrainingDueNoticeValidator(List<IManualTrainingDueNoticeRule> Rules)
{
    public List<string> Validate(ManualTrainingDueNotice item)
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

    public void RequireValid(ManualTrainingDueNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualTrainingDueNotice> items)
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

/// <summary>Functions of manual training due notice rules.</summary>
public static class ManualTrainingDueNoticeRules
{
    public static ManualTrainingDueNoticeValidator DefaultManualTrainingDueNoticeValidator()
    {
        return new ManualTrainingDueNoticeValidator(
            new List<IManualTrainingDueNoticeRule>
            {
                new ManualTrainingDueNoticeEmployeeIdLengthRule(120),
                new ManualTrainingDueNoticeCodePrefixRule("X"),
                new ManualTrainingDueNoticeCodeLengthRule(3600),
                new ManualTrainingDueNoticeNameLengthRule(16),
                new ManualTrainingDueNoticeCapacityRangeRule(32, 80),
                new ManualTrainingDueNoticeRequiredCodeRule("manual training due notice needs an entry"),
            });
    }
}
