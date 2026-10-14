namespace Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>A rule a adjustment exception must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentExceptionRule
{
    List<string> Check(AdjustmentException item);
}

public sealed record AdjustmentExceptionRequiredCodeRule(string Message) : IAdjustmentExceptionRule
{
    public List<string> Check(AdjustmentException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentExceptionRatioRangeRule(double Min, double Max) : IAdjustmentExceptionRule
{
    public List<string> Check(AdjustmentException item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentExceptionRequiredContactEmailRule(string Message) : IAdjustmentExceptionRule
{
    public List<string> Check(AdjustmentException item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentExceptionVersionRangeRule(int Min, int Max) : IAdjustmentExceptionRule
{
    public List<string> Check(AdjustmentException item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentExceptionContactEmailLengthRule(int MaxLength) : IAdjustmentExceptionRule
{
    public List<string> Check(AdjustmentException item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentExceptionTitleLengthRule(int MaxLength) : IAdjustmentExceptionRule
{
    public List<string> Check(AdjustmentException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment exception.</summary>
public sealed record AdjustmentExceptionValidator(List<IAdjustmentExceptionRule> Rules)
{
    public List<string> Validate(AdjustmentException item)
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

    public void RequireValid(AdjustmentException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentException> items)
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

/// <summary>Functions of adjustment exception rules.</summary>
public static class AdjustmentExceptionRules
{
    public static AdjustmentExceptionValidator DefaultAdjustmentExceptionValidator()
    {
        return new AdjustmentExceptionValidator(
            new List<IAdjustmentExceptionRule>
            {
                new AdjustmentExceptionRequiredCodeRule("adjustment exception needs this field"),
                new AdjustmentExceptionRatioRangeRule(8.6, 3131.7),
                new AdjustmentExceptionRequiredContactEmailRule("adjustment exception needs an entry"),
                new AdjustmentExceptionVersionRangeRule(16, 80),
                new AdjustmentExceptionContactEmailLengthRule(255),
                new AdjustmentExceptionTitleLengthRule(3600),
            });
    }
}
