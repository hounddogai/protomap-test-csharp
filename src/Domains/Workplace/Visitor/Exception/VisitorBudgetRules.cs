namespace Acme.Workforce.Workplace.Visitor.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor budget must pass. Check returns its problems, or none.</summary>
public interface IVisitorBudgetRule
{
    List<string> Check(VisitorBudget item);
}

public sealed record VisitorBudgetWeightRangeRule(double Min, double Max) : IVisitorBudgetRule
{
    public List<string> Check(VisitorBudget item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorBudgetCodeLengthRule(int MaxLength) : IVisitorBudgetRule
{
    public List<string> Check(VisitorBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorBudgetAllowedContactEmailRule(List<string> Allowed) : IVisitorBudgetRule
{
    public List<string> Check(VisitorBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor budget.</summary>
public sealed record VisitorBudgetValidator(List<IVisitorBudgetRule> Rules)
{
    public List<string> Validate(VisitorBudget item)
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

    public void RequireValid(VisitorBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorBudget> items)
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

/// <summary>Functions of visitor budget rules.</summary>
public static class VisitorBudgetRules
{
    public static VisitorBudgetValidator DefaultVisitorBudgetValidator()
    {
        return new VisitorBudgetValidator(
            new List<IVisitorBudgetRule>
            {
                new VisitorBudgetWeightRangeRule(4.9, 1730.0),
                new VisitorBudgetCodeLengthRule(16),
                new VisitorBudgetAllowedContactEmailRule(new List<string>()),
            });
    }
}
