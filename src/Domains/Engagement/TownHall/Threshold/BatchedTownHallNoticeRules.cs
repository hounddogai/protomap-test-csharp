namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched town hall notice must pass. Check returns its problems, or none.</summary>
public interface IBatchedTownHallNoticeRule
{
    List<string> Check(BatchedTownHallNotice item);
}

public sealed record BatchedTownHallNoticeAllowedJobCodeRule(List<string> Allowed) : IBatchedTownHallNoticeRule
{
    public List<string> Check(BatchedTownHallNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallNoticeNameLengthRule(int MaxLength) : IBatchedTownHallNoticeRule
{
    public List<string> Check(BatchedTownHallNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallNoticeUnitLengthRule(int MaxLength) : IBatchedTownHallNoticeRule
{
    public List<string> Check(BatchedTownHallNotice item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallNoticePinnedGuardRule(double Limit) : IBatchedTownHallNoticeRule
{
    public List<string> Check(BatchedTownHallNotice item)
    {
        if (item.Pinned && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallNoticeCodePrefixRule(string Prefix) : IBatchedTownHallNoticeRule
{
    public List<string> Check(BatchedTownHallNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallNoticeRequiredChannelRule(string Message) : IBatchedTownHallNoticeRule
{
    public List<string> Check(BatchedTownHallNotice item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched town hall notice.</summary>
public sealed record BatchedTownHallNoticeValidator(List<IBatchedTownHallNoticeRule> Rules)
{
    public List<string> Validate(BatchedTownHallNotice item)
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

    public void RequireValid(BatchedTownHallNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTownHallNotice> items)
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

/// <summary>Functions of batched town hall notice rules.</summary>
public static class BatchedTownHallNoticeRules
{
    public static BatchedTownHallNoticeValidator DefaultBatchedTownHallNoticeValidator()
    {
        return new BatchedTownHallNoticeValidator(
            new List<IBatchedTownHallNoticeRule>
            {
                new BatchedTownHallNoticeAllowedJobCodeRule(new List<string> { "default" }),
                new BatchedTownHallNoticeNameLengthRule(64),
                new BatchedTownHallNoticeUnitLengthRule(86400),
                new BatchedTownHallNoticePinnedGuardRule(0.9),
                new BatchedTownHallNoticeCodePrefixRule("ACM"),
                new BatchedTownHallNoticeRequiredChannelRule("batched town hall notice needs a value"),
            });
    }
}
