namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default desk budget must pass. Check returns its problems, or none.</summary>
public interface IDefaultDeskBudgetRule
{
    List<string> Check(DefaultDeskBudget item);
}

public sealed record DefaultDeskBudgetRequiredTitleRule(string Message) : IDefaultDeskBudgetRule
{
    public List<string> Check(DefaultDeskBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskBudgetRequiredManagerIdRule(string Message) : IDefaultDeskBudgetRule
{
    public List<string> Check(DefaultDeskBudget item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskBudgetManagerIdLengthRule(int MaxLength) : IDefaultDeskBudgetRule
{
    public List<string> Check(DefaultDeskBudget item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskBudgetCodeLengthRule(int MaxLength) : IDefaultDeskBudgetRule
{
    public List<string> Check(DefaultDeskBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskBudgetAllowedManagerIdRule(List<string> Allowed) : IDefaultDeskBudgetRule
{
    public List<string> Check(DefaultDeskBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is stale" };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskBudgetCodePrefixRule(string Prefix) : IDefaultDeskBudgetRule
{
    public List<string> Check(DefaultDeskBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default desk budget.</summary>
public sealed record DefaultDeskBudgetValidator(List<IDefaultDeskBudgetRule> Rules)
{
    public List<string> Validate(DefaultDeskBudget item)
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

    public void RequireValid(DefaultDeskBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultDeskBudget> items)
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

/// <summary>Functions of default desk budget rules.</summary>
public static class DefaultDeskBudgetRules
{
    public static DefaultDeskBudgetValidator DefaultDefaultDeskBudgetValidator()
    {
        return new DefaultDeskBudgetValidator(
            new List<IDefaultDeskBudgetRule>
            {
                new DefaultDeskBudgetRequiredTitleRule("default desk budget needs an entry"),
                new DefaultDeskBudgetRequiredManagerIdRule("default desk budget needs a value"),
                new DefaultDeskBudgetManagerIdLengthRule(80),
                new DefaultDeskBudgetCodeLengthRule(3600),
                new DefaultDeskBudgetAllowedManagerIdRule(new List<string> { "batched" }),
                new DefaultDeskBudgetCodePrefixRule("OPS"),
            });
    }
}
