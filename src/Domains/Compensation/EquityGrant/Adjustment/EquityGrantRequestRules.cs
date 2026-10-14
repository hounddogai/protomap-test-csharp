namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant request must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantRequestRule
{
    List<string> Check(EquityGrantRequest item);
}

public sealed record EquityGrantRequestNameLengthRule(int MaxLength) : IEquityGrantRequestRule
{
    public List<string> Check(EquityGrantRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRequestNotesLengthRule(int MaxLength) : IEquityGrantRequestRule
{
    public List<string> Check(EquityGrantRequest item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRequestAllowedNameRule(List<string> Allowed) : IEquityGrantRequestRule
{
    public List<string> Check(EquityGrantRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRequestTitleLengthRule(int MaxLength) : IEquityGrantRequestRule
{
    public List<string> Check(EquityGrantRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant request.</summary>
public sealed record EquityGrantRequestValidator(List<IEquityGrantRequestRule> Rules)
{
    public List<string> Validate(EquityGrantRequest item)
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

    public void RequireValid(EquityGrantRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantRequest> items)
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

/// <summary>Functions of equity grant request rules.</summary>
public static class EquityGrantRequestRules
{
    public static EquityGrantRequestValidator DefaultEquityGrantRequestValidator()
    {
        return new EquityGrantRequestValidator(
            new List<IEquityGrantRequestRule>
            {
                new EquityGrantRequestNameLengthRule(255),
                new EquityGrantRequestNotesLengthRule(16),
                new EquityGrantRequestAllowedNameRule(new List<string> { "scheduled" }),
                new EquityGrantRequestTitleLengthRule(80),
            });
    }
}
