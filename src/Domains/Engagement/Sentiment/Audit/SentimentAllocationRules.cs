namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment allocation must pass. Check returns its problems, or none.</summary>
public interface ISentimentAllocationRule
{
    List<string> Check(SentimentAllocation item);
}

public sealed record SentimentAllocationAllowedNameRule(List<string> Allowed) : ISentimentAllocationRule
{
    public List<string> Check(SentimentAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAllocationNameLengthRule(int MaxLength) : ISentimentAllocationRule
{
    public List<string> Check(SentimentAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAllocationCodeLengthRule(int MaxLength) : ISentimentAllocationRule
{
    public List<string> Check(SentimentAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAllocationRateRangeRule(double Min, double Max) : ISentimentAllocationRule
{
    public List<string> Check(SentimentAllocation item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a sentiment allocation.</summary>
public sealed record SentimentAllocationValidator(List<ISentimentAllocationRule> Rules)
{
    public List<string> Validate(SentimentAllocation item)
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

    public void RequireValid(SentimentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentAllocation> items)
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

/// <summary>Functions of sentiment allocation rules.</summary>
public static class SentimentAllocationRules
{
    public static SentimentAllocationValidator DefaultSentimentAllocationValidator()
    {
        return new SentimentAllocationValidator(
            new List<ISentimentAllocationRule>
            {
                new SentimentAllocationAllowedNameRule(new List<string>()),
                new SentimentAllocationNameLengthRule(120),
                new SentimentAllocationCodeLengthRule(80),
                new SentimentAllocationRateRangeRule(0.7, 622.4),
            });
    }
}
