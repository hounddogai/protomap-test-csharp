namespace Acme.Workforce.Absence.Accrual.Slot;

/// <summary>A rule a accrual template must pass. Check returns its problems, or none.</summary>
public interface IAccrualTemplateRule
{
    List<string> Check(AccrualTemplate item);
}

public sealed record AccrualTemplateRequiredCodeRule(string Message) : IAccrualTemplateRule
{
    public List<string> Check(AccrualTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualTemplateRateRangeRule(double Min, double Max) : IAccrualTemplateRule
{
    public List<string> Check(AccrualTemplate item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualTemplateSegmentLengthRule(int MaxLength) : IAccrualTemplateRule
{
    public List<string> Check(AccrualTemplate item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualTemplateRequiredSegmentRule(string Message) : IAccrualTemplateRule
{
    public List<string> Check(AccrualTemplate item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual template.</summary>
public sealed record AccrualTemplateValidator(List<IAccrualTemplateRule> Rules)
{
    public List<string> Validate(AccrualTemplate item)
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

    public void RequireValid(AccrualTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualTemplate> items)
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

/// <summary>Functions of accrual template rules.</summary>
public static class AccrualTemplateRules
{
    public static AccrualTemplateValidator DefaultAccrualTemplateValidator()
    {
        return new AccrualTemplateValidator(
            new List<IAccrualTemplateRule>
            {
                new AccrualTemplateRequiredCodeRule("accrual template needs a value"),
                new AccrualTemplateRateRangeRule(6.1, 1497.0),
                new AccrualTemplateSegmentLengthRule(32),
                new AccrualTemplateRequiredSegmentRule("accrual template needs a value"),
            });
    }
}
