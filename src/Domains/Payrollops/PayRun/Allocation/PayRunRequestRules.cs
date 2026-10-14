namespace Acme.Workforce.Payrollops.PayRun.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay run request must pass. Check returns its problems, or none.</summary>
public interface IPayRunRequestRule
{
    List<string> Check(PayRunRequest item);
}

public sealed record PayRunRequestCodePrefixRule(string Prefix) : IPayRunRequestRule
{
    public List<string> Check(PayRunRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunRequestCodeLengthRule(int MaxLength) : IPayRunRequestRule
{
    public List<string> Check(PayRunRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunRequestLabelLengthRule(int MaxLength) : IPayRunRequestRule
{
    public List<string> Check(PayRunRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunRequestAllowedBadgeRule(List<string> Allowed) : IPayRunRequestRule
{
    public List<string> Check(PayRunRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PayRunRequestRequiredLabelRule(string Message) : IPayRunRequestRule
{
    public List<string> Check(PayRunRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunRequestBudgetRangeRule(double Min, double Max) : IPayRunRequestRule
{
    public List<string> Check(PayRunRequest item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run request.</summary>
public sealed record PayRunRequestValidator(List<IPayRunRequestRule> Rules)
{
    public List<string> Validate(PayRunRequest item)
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

    public void RequireValid(PayRunRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunRequest> items)
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

/// <summary>Functions of pay run request rules.</summary>
public static class PayRunRequestRules
{
    public static PayRunRequestValidator DefaultPayRunRequestValidator()
    {
        return new PayRunRequestValidator(
            new List<IPayRunRequestRule>
            {
                new PayRunRequestCodePrefixRule("Q"),
                new PayRunRequestCodeLengthRule(64),
                new PayRunRequestLabelLengthRule(120),
                new PayRunRequestAllowedBadgeRule(new List<string> { "weighted" }),
                new PayRunRequestRequiredLabelRule("pay run request needs an entry"),
                new PayRunRequestBudgetRangeRule(2.3, 482.1),
            });
    }
}
