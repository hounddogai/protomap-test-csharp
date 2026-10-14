namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position quota must pass. Check returns its problems, or none.</summary>
public interface IPositionQuotaRule
{
    List<string> Check(PositionQuota item);
}

public sealed record PositionQuotaRequiredCodeRule(string Message) : IPositionQuotaRule
{
    public List<string> Check(PositionQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionQuotaAllowedChannelRule(List<string> Allowed) : IPositionQuotaRule
{
    public List<string> Check(PositionQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PositionQuotaCodeLengthRule(int MaxLength) : IPositionQuotaRule
{
    public List<string> Check(PositionQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionQuotaAmountRangeRule(double Min, double Max) : IPositionQuotaRule
{
    public List<string> Check(PositionQuota item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionQuotaChannelLengthRule(int MaxLength) : IPositionQuotaRule
{
    public List<string> Check(PositionQuota item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position quota.</summary>
public sealed record PositionQuotaValidator(List<IPositionQuotaRule> Rules)
{
    public List<string> Validate(PositionQuota item)
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

    public void RequireValid(PositionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionQuota> items)
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

/// <summary>Functions of position quota rules.</summary>
public static class PositionQuotaRules
{
    public static PositionQuotaValidator DefaultPositionQuotaValidator()
    {
        return new PositionQuotaValidator(
            new List<IPositionQuotaRule>
            {
                new PositionQuotaRequiredCodeRule("position quota needs an entry"),
                new PositionQuotaAllowedChannelRule(new List<string> { "weighted", "capped" }),
                new PositionQuotaCodeLengthRule(64),
                new PositionQuotaAmountRangeRule(2.5, 1506.6),
                new PositionQuotaChannelLengthRule(255),
            });
    }
}
