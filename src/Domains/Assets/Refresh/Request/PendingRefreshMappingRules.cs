namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending refresh mapping must pass. Check returns its problems, or none.</summary>
public interface IPendingRefreshMappingRule
{
    List<string> Check(PendingRefreshMapping item);
}

public sealed record PendingRefreshMappingLimitRangeRule(int Min, int Max) : IPendingRefreshMappingRule
{
    public List<string> Check(PendingRefreshMapping item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshMappingLabelLengthRule(int MaxLength) : IPendingRefreshMappingRule
{
    public List<string> Check(PendingRefreshMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshMappingAllowedCodeRule(List<string> Allowed) : IPendingRefreshMappingRule
{
    public List<string> Check(PendingRefreshMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshMappingRequiredCodeRule(string Message) : IPendingRefreshMappingRule
{
    public List<string> Check(PendingRefreshMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending refresh mapping.</summary>
public sealed record PendingRefreshMappingValidator(List<IPendingRefreshMappingRule> Rules)
{
    public List<string> Validate(PendingRefreshMapping item)
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

    public void RequireValid(PendingRefreshMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRefreshMapping> items)
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

/// <summary>Functions of pending refresh mapping rules.</summary>
public static class PendingRefreshMappingRules
{
    public static PendingRefreshMappingValidator DefaultPendingRefreshMappingValidator()
    {
        return new PendingRefreshMappingValidator(
            new List<IPendingRefreshMappingRule>
            {
                new PendingRefreshMappingLimitRangeRule(64, 3600),
                new PendingRefreshMappingLabelLengthRule(80),
                new PendingRefreshMappingAllowedCodeRule(new List<string> { "manual", "lenient" }),
                new PendingRefreshMappingRequiredCodeRule("pending refresh mapping needs an entry"),
            });
    }
}
