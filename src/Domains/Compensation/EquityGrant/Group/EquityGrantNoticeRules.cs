namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant notice must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantNoticeRule
{
    List<string> Check(EquityGrantNotice item);
}

public sealed record EquityGrantNoticeCodePrefixRule(string Prefix) : IEquityGrantNoticeRule
{
    public List<string> Check(EquityGrantNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantNoticeCodeLengthRule(int MaxLength) : IEquityGrantNoticeRule
{
    public List<string> Check(EquityGrantNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantNoticeRequiredTimezoneRule(string Message) : IEquityGrantNoticeRule
{
    public List<string> Check(EquityGrantNotice item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantNoticeBudgetRangeRule(double Min, double Max) : IEquityGrantNoticeRule
{
    public List<string> Check(EquityGrantNotice item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantNoticeRequiredCodeRule(string Message) : IEquityGrantNoticeRule
{
    public List<string> Check(EquityGrantNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantNoticeAllowedTitleRule(List<string> Allowed) : IEquityGrantNoticeRule
{
    public List<string> Check(EquityGrantNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant notice.</summary>
public sealed record EquityGrantNoticeValidator(List<IEquityGrantNoticeRule> Rules)
{
    public List<string> Validate(EquityGrantNotice item)
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

    public void RequireValid(EquityGrantNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantNotice> items)
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

/// <summary>Functions of equity grant notice rules.</summary>
public static class EquityGrantNoticeRules
{
    public static EquityGrantNoticeValidator DefaultEquityGrantNoticeValidator()
    {
        return new EquityGrantNoticeValidator(
            new List<IEquityGrantNoticeRule>
            {
                new EquityGrantNoticeCodePrefixRule("ACM"),
                new EquityGrantNoticeCodeLengthRule(32),
                new EquityGrantNoticeRequiredTimezoneRule("equity grant notice needs this field"),
                new EquityGrantNoticeBudgetRangeRule(0.4, 765.2),
                new EquityGrantNoticeRequiredCodeRule("equity grant notice needs this field"),
                new EquityGrantNoticeAllowedTitleRule(new List<string>()),
            });
    }
}
