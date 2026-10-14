namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict rate notice must pass. Check returns its problems, or none.</summary>
public interface IStrictRateNoticeRule
{
    List<string> Check(StrictRateNotice item);
}

public sealed record StrictRateNoticeCodeLengthRule(int MaxLength) : IStrictRateNoticeRule
{
    public List<string> Check(StrictRateNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRateNoticeAllowedNameRule(List<string> Allowed) : IStrictRateNoticeRule
{
    public List<string> Check(StrictRateNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record StrictRateNoticeMinimumRangeRule(double Min, double Max) : IStrictRateNoticeRule
{
    public List<string> Check(StrictRateNotice item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRateNoticeEndsAtRangeRule(int Min, int Max) : IStrictRateNoticeRule
{
    public List<string> Check(StrictRateNotice item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRateNoticeCreatedAtRangeRule(int Min, int Max) : IStrictRateNoticeRule
{
    public List<string> Check(StrictRateNotice item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict rate notice.</summary>
public sealed record StrictRateNoticeValidator(List<IStrictRateNoticeRule> Rules)
{
    public List<string> Validate(StrictRateNotice item)
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

    public void RequireValid(StrictRateNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictRateNotice> items)
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

/// <summary>Functions of strict rate notice rules.</summary>
public static class StrictRateNoticeRules
{
    public static StrictRateNoticeValidator DefaultStrictRateNoticeValidator()
    {
        return new StrictRateNoticeValidator(
            new List<IStrictRateNoticeRule>
            {
                new StrictRateNoticeCodeLengthRule(64),
                new StrictRateNoticeAllowedNameRule(new List<string> { "batched" }),
                new StrictRateNoticeMinimumRangeRule(5.4, 4696.9),
                new StrictRateNoticeEndsAtRangeRule(32, 86400),
                new StrictRateNoticeCreatedAtRangeRule(64, 3600),
            });
    }
}
