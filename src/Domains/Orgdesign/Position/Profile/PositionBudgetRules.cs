namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position budget must pass. Check returns its problems, or none.</summary>
public interface IPositionBudgetRule
{
    List<string> Check(PositionBudget item);
}

public sealed record PositionBudgetAllowedTimezoneRule(List<string> Allowed) : IPositionBudgetRule
{
    public List<string> Check(PositionBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record PositionBudgetCodePrefixRule(string Prefix) : IPositionBudgetRule
{
    public List<string> Check(PositionBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionBudgetRequiredCodeRule(string Message) : IPositionBudgetRule
{
    public List<string> Check(PositionBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionBudgetRequiredReviewerRule(string Message) : IPositionBudgetRule
{
    public List<string> Check(PositionBudget item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position budget.</summary>
public sealed record PositionBudgetValidator(List<IPositionBudgetRule> Rules)
{
    public List<string> Validate(PositionBudget item)
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

    public void RequireValid(PositionBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionBudget> items)
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

/// <summary>Functions of position budget rules.</summary>
public static class PositionBudgetRules
{
    public static PositionBudgetValidator DefaultPositionBudgetValidator()
    {
        return new PositionBudgetValidator(
            new List<IPositionBudgetRule>
            {
                new PositionBudgetAllowedTimezoneRule(new List<string> { "rolling" }),
                new PositionBudgetCodePrefixRule("ACM"),
                new PositionBudgetRequiredCodeRule("position budget needs an entry"),
                new PositionBudgetRequiredReviewerRule("position budget needs a value"),
            });
    }
}
