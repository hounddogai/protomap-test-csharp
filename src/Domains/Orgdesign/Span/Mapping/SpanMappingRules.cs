namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span mapping must pass. Check returns its problems, or none.</summary>
public interface ISpanMappingRule
{
    List<string> Check(SpanMapping item);
}

public sealed record SpanMappingLeaveDaysRangeRule(int Min, int Max) : ISpanMappingRule
{
    public List<string> Check(SpanMapping item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanMappingActiveGuardRule(int Limit) : ISpanMappingRule
{
    public List<string> Check(SpanMapping item)
    {
        if (item.Active && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record SpanMappingRequiredCodeRule(string Message) : ISpanMappingRule
{
    public List<string> Check(SpanMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanMappingLabelLengthRule(int MaxLength) : ISpanMappingRule
{
    public List<string> Check(SpanMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanMappingChannelLengthRule(int MaxLength) : ISpanMappingRule
{
    public List<string> Check(SpanMapping item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanMappingAllowedTeamRule(List<string> Allowed) : ISpanMappingRule
{
    public List<string> Check(SpanMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span mapping.</summary>
public sealed record SpanMappingValidator(List<ISpanMappingRule> Rules)
{
    public List<string> Validate(SpanMapping item)
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

    public void RequireValid(SpanMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanMapping> items)
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

/// <summary>Functions of span mapping rules.</summary>
public static class SpanMappingRules
{
    public static SpanMappingValidator DefaultSpanMappingValidator()
    {
        return new SpanMappingValidator(
            new List<ISpanMappingRule>
            {
                new SpanMappingLeaveDaysRangeRule(32, 86400),
                new SpanMappingActiveGuardRule(64),
                new SpanMappingRequiredCodeRule("span mapping needs an entry"),
                new SpanMappingLabelLengthRule(3600),
                new SpanMappingChannelLengthRule(1000),
                new SpanMappingAllowedTeamRule(new List<string>()),
            });
    }
}
