namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped pay run target must pass. Check returns its problems, or none.</summary>
public interface ICappedPayRunTargetRule
{
    List<string> Check(CappedPayRunTarget item);
}

public sealed record CappedPayRunTargetAmountRangeRule(double Min, double Max) : ICappedPayRunTargetRule
{
    public List<string> Check(CappedPayRunTarget item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunTargetChannelLengthRule(int MaxLength) : ICappedPayRunTargetRule
{
    public List<string> Check(CappedPayRunTarget item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunTargetAllowedTitleRule(List<string> Allowed) : ICappedPayRunTargetRule
{
    public List<string> Check(CappedPayRunTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunTargetRequiredTitleRule(string Message) : ICappedPayRunTargetRule
{
    public List<string> Check(CappedPayRunTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunTargetRequiredChannelRule(string Message) : ICappedPayRunTargetRule
{
    public List<string> Check(CappedPayRunTarget item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunTargetCurrencyLengthRule(int MaxLength) : ICappedPayRunTargetRule
{
    public List<string> Check(CappedPayRunTarget item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pay run target.</summary>
public sealed record CappedPayRunTargetValidator(List<ICappedPayRunTargetRule> Rules)
{
    public List<string> Validate(CappedPayRunTarget item)
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

    public void RequireValid(CappedPayRunTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayRunTarget> items)
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

/// <summary>Functions of capped pay run target rules.</summary>
public static class CappedPayRunTargetRules
{
    public static CappedPayRunTargetValidator DefaultCappedPayRunTargetValidator()
    {
        return new CappedPayRunTargetValidator(
            new List<ICappedPayRunTargetRule>
            {
                new CappedPayRunTargetAmountRangeRule(7.4, 4832.9),
                new CappedPayRunTargetChannelLengthRule(16),
                new CappedPayRunTargetAllowedTitleRule(new List<string> { "rolling", "global", "legacy" }),
                new CappedPayRunTargetRequiredTitleRule("capped pay run target needs an entry"),
                new CappedPayRunTargetRequiredChannelRule("capped pay run target needs this field"),
                new CappedPayRunTargetCurrencyLengthRule(64),
            });
    }
}
