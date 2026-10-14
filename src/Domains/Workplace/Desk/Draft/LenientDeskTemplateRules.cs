namespace Acme.Workforce.Workplace.Desk.Draft;

/// <summary>A rule a lenient desk template must pass. Check returns its problems, or none.</summary>
public interface ILenientDeskTemplateRule
{
    List<string> Check(LenientDeskTemplate item);
}

public sealed record LenientDeskTemplateRequiredChannelRule(string Message) : ILenientDeskTemplateRule
{
    public List<string> Check(LenientDeskTemplate item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskTemplateCodeLengthRule(int MaxLength) : ILenientDeskTemplateRule
{
    public List<string> Check(LenientDeskTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskTemplateLeaveDaysRangeRule(int Min, int Max) : ILenientDeskTemplateRule
{
    public List<string> Check(LenientDeskTemplate item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskTemplateApprovedGuardRule(int Limit) : ILenientDeskTemplateRule
{
    public List<string> Check(LenientDeskTemplate item)
    {
        if (item.Approved && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskTemplateRequiredNameRule(string Message) : ILenientDeskTemplateRule
{
    public List<string> Check(LenientDeskTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskTemplateRequiredJobCodeRule(string Message) : ILenientDeskTemplateRule
{
    public List<string> Check(LenientDeskTemplate item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient desk template.</summary>
public sealed record LenientDeskTemplateValidator(List<ILenientDeskTemplateRule> Rules)
{
    public List<string> Validate(LenientDeskTemplate item)
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

    public void RequireValid(LenientDeskTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeskTemplate> items)
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

/// <summary>Functions of lenient desk template rules.</summary>
public static class LenientDeskTemplateRules
{
    public static LenientDeskTemplateValidator DefaultLenientDeskTemplateValidator()
    {
        return new LenientDeskTemplateValidator(
            new List<ILenientDeskTemplateRule>
            {
                new LenientDeskTemplateRequiredChannelRule("lenient desk template needs a value"),
                new LenientDeskTemplateCodeLengthRule(255),
                new LenientDeskTemplateLeaveDaysRangeRule(255, 1000),
                new LenientDeskTemplateApprovedGuardRule(3600),
                new LenientDeskTemplateRequiredNameRule("lenient desk template needs this field"),
                new LenientDeskTemplateRequiredJobCodeRule("lenient desk template needs an entry"),
            });
    }
}
