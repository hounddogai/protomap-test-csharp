namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default pay run request must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayRunRequestRule
{
    List<string> Check(DefaultPayRunRequest item);
}

public sealed record DefaultPayRunRequestCodePrefixRule(string Prefix) : IDefaultPayRunRequestRule
{
    public List<string> Check(DefaultPayRunRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunRequestWindowDaysRangeRule(int Min, int Max) : IDefaultPayRunRequestRule
{
    public List<string> Check(DefaultPayRunRequest item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunRequestChannelLengthRule(int MaxLength) : IDefaultPayRunRequestRule
{
    public List<string> Check(DefaultPayRunRequest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunRequestAllowedExternalRefRule(List<string> Allowed) : IDefaultPayRunRequestRule
{
    public List<string> Check(DefaultPayRunRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is too long" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunRequestVersionRangeRule(int Min, int Max) : IDefaultPayRunRequestRule
{
    public List<string> Check(DefaultPayRunRequest item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pay run request.</summary>
public sealed record DefaultPayRunRequestValidator(List<IDefaultPayRunRequestRule> Rules)
{
    public List<string> Validate(DefaultPayRunRequest item)
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

    public void RequireValid(DefaultPayRunRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayRunRequest> items)
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

/// <summary>Functions of default pay run request rules.</summary>
public static class DefaultPayRunRequestRules
{
    public static DefaultPayRunRequestValidator DefaultDefaultPayRunRequestValidator()
    {
        return new DefaultPayRunRequestValidator(
            new List<IDefaultPayRunRequestRule>
            {
                new DefaultPayRunRequestCodePrefixRule("ACM"),
                new DefaultPayRunRequestWindowDaysRangeRule(16, 32),
                new DefaultPayRunRequestChannelLengthRule(255),
                new DefaultPayRunRequestAllowedExternalRefRule(new List<string>()),
                new DefaultPayRunRequestVersionRangeRule(120, 86400),
            });
    }
}
