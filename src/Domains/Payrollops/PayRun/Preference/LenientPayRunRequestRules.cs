namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient pay run request must pass. Check returns its problems, or none.</summary>
public interface ILenientPayRunRequestRule
{
    List<string> Check(LenientPayRunRequest item);
}

public sealed record LenientPayRunRequestChannelLengthRule(int MaxLength) : ILenientPayRunRequestRule
{
    public List<string> Check(LenientPayRunRequest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunRequestVersionRangeRule(int Min, int Max) : ILenientPayRunRequestRule
{
    public List<string> Check(LenientPayRunRequest item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientPayRunRequestRequiredCodeRule(string Message) : ILenientPayRunRequestRule
{
    public List<string> Check(LenientPayRunRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunRequestAllowedNameRule(List<string> Allowed) : ILenientPayRunRequestRule
{
    public List<string> Check(LenientPayRunRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient pay run request.</summary>
public sealed record LenientPayRunRequestValidator(List<ILenientPayRunRequestRule> Rules)
{
    public List<string> Validate(LenientPayRunRequest item)
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

    public void RequireValid(LenientPayRunRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPayRunRequest> items)
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

/// <summary>Functions of lenient pay run request rules.</summary>
public static class LenientPayRunRequestRules
{
    public static LenientPayRunRequestValidator DefaultLenientPayRunRequestValidator()
    {
        return new LenientPayRunRequestValidator(
            new List<ILenientPayRunRequestRule>
            {
                new LenientPayRunRequestChannelLengthRule(3600),
                new LenientPayRunRequestVersionRangeRule(64, 120),
                new LenientPayRunRequestRequiredCodeRule("lenient pay run request needs this field"),
                new LenientPayRunRequestAllowedNameRule(new List<string> { "manual", "primary" }),
            });
    }
}
