namespace Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>A rule a fallback adjustment draft must pass. Check returns its problems, or none.</summary>
public interface IFallbackAdjustmentDraftRule
{
    List<string> Check(FallbackAdjustmentDraft item);
}

public sealed record FallbackAdjustmentDraftRequiredCodeRule(string Message) : IFallbackAdjustmentDraftRule
{
    public List<string> Check(FallbackAdjustmentDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAdjustmentDraftRetriesRangeRule(int Min, int Max) : IFallbackAdjustmentDraftRule
{
    public List<string> Check(FallbackAdjustmentDraft item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackAdjustmentDraftSalaryBandLengthRule(int MaxLength) : IFallbackAdjustmentDraftRule
{
    public List<string> Check(FallbackAdjustmentDraft item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAdjustmentDraftEmployeeIdLengthRule(int MaxLength) : IFallbackAdjustmentDraftRule
{
    public List<string> Check(FallbackAdjustmentDraft item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAdjustmentDraftNameLengthRule(int MaxLength) : IFallbackAdjustmentDraftRule
{
    public List<string> Check(FallbackAdjustmentDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAdjustmentDraftVersionRangeRule(int Min, int Max) : IFallbackAdjustmentDraftRule
{
    public List<string> Check(FallbackAdjustmentDraft item)
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

/// <summary>Runs every rule of a fallback adjustment draft.</summary>
public sealed record FallbackAdjustmentDraftValidator(List<IFallbackAdjustmentDraftRule> Rules)
{
    public List<string> Validate(FallbackAdjustmentDraft item)
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

    public void RequireValid(FallbackAdjustmentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAdjustmentDraft> items)
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

/// <summary>Functions of fallback adjustment draft rules.</summary>
public static class FallbackAdjustmentDraftRules
{
    public static FallbackAdjustmentDraftValidator DefaultFallbackAdjustmentDraftValidator()
    {
        return new FallbackAdjustmentDraftValidator(
            new List<IFallbackAdjustmentDraftRule>
            {
                new FallbackAdjustmentDraftRequiredCodeRule("fallback adjustment draft needs a value"),
                new FallbackAdjustmentDraftRetriesRangeRule(16, 80),
                new FallbackAdjustmentDraftSalaryBandLengthRule(3600),
                new FallbackAdjustmentDraftEmployeeIdLengthRule(86400),
                new FallbackAdjustmentDraftNameLengthRule(80),
                new FallbackAdjustmentDraftVersionRangeRule(32, 32),
            });
    }
}
