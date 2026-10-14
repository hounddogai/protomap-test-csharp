namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg request must pass. Check returns its problems, or none.</summary>
public interface IReorgRequestRule
{
    List<string> Check(ReorgRequest item);
}

public sealed record ReorgRequestTitleLengthRule(int MaxLength) : IReorgRequestRule
{
    public List<string> Check(ReorgRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgRequestCodePrefixRule(string Prefix) : IReorgRequestRule
{
    public List<string> Check(ReorgRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgRequestAllowedSourceRule(List<string> Allowed) : IReorgRequestRule
{
    public List<string> Check(ReorgRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ReorgRequestManagerIdLengthRule(int MaxLength) : IReorgRequestRule
{
    public List<string> Check(ReorgRequest item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg request.</summary>
public sealed record ReorgRequestValidator(List<IReorgRequestRule> Rules)
{
    public List<string> Validate(ReorgRequest item)
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

    public void RequireValid(ReorgRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgRequest> items)
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

/// <summary>Functions of reorg request rules.</summary>
public static class ReorgRequestRules
{
    public static ReorgRequestValidator DefaultReorgRequestValidator()
    {
        return new ReorgRequestValidator(
            new List<IReorgRequestRule>
            {
                new ReorgRequestTitleLengthRule(32),
                new ReorgRequestCodePrefixRule("Q"),
                new ReorgRequestAllowedSourceRule(new List<string>()),
                new ReorgRequestManagerIdLengthRule(86400),
            });
    }
}
