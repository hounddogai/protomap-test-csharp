namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted rate notice must pass. Check returns its problems, or none.</summary>
public interface IWeightedRateNoticeRule
{
    List<string> Check(WeightedRateNotice item);
}

public sealed record WeightedRateNoticeAllowedContactEmailRule(List<string> Allowed) : IWeightedRateNoticeRule
{
    public List<string> Check(WeightedRateNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateNoticeCodeLengthRule(int MaxLength) : IWeightedRateNoticeRule
{
    public List<string> Check(WeightedRateNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateNoticeRetriesRangeRule(int Min, int Max) : IWeightedRateNoticeRule
{
    public List<string> Check(WeightedRateNotice item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted rate notice.</summary>
public sealed record WeightedRateNoticeValidator(List<IWeightedRateNoticeRule> Rules)
{
    public List<string> Validate(WeightedRateNotice item)
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

    public void RequireValid(WeightedRateNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRateNotice> items)
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

/// <summary>Functions of weighted rate notice rules.</summary>
public static class WeightedRateNoticeRules
{
    public static WeightedRateNoticeValidator DefaultWeightedRateNoticeValidator()
    {
        return new WeightedRateNoticeValidator(
            new List<IWeightedRateNoticeRule>
            {
                new WeightedRateNoticeAllowedContactEmailRule(new List<string> { "scheduled", "strict" }),
                new WeightedRateNoticeCodeLengthRule(86400),
                new WeightedRateNoticeRetriesRangeRule(32, 32),
            });
    }
}
