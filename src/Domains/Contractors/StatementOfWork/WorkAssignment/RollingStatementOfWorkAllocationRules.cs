namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling statement of work allocation must pass. Check returns its problems, or none.</summary>
public interface IRollingStatementOfWorkAllocationRule
{
    List<string> Check(RollingStatementOfWorkAllocation item);
}

public sealed record RollingStatementOfWorkAllocationAllowedCodeRule(List<string> Allowed) : IRollingStatementOfWorkAllocationRule
{
    public List<string> Check(RollingStatementOfWorkAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RollingStatementOfWorkAllocationCodePrefixRule(string Prefix) : IRollingStatementOfWorkAllocationRule
{
    public List<string> Check(RollingStatementOfWorkAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingStatementOfWorkAllocationCeilingRangeRule(double Min, double Max) : IRollingStatementOfWorkAllocationRule
{
    public List<string> Check(RollingStatementOfWorkAllocation item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingStatementOfWorkAllocationRequiredCodeRule(string Message) : IRollingStatementOfWorkAllocationRule
{
    public List<string> Check(RollingStatementOfWorkAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingStatementOfWorkAllocationDiscountRangeRule(double Min, double Max) : IRollingStatementOfWorkAllocationRule
{
    public List<string> Check(RollingStatementOfWorkAllocation item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingStatementOfWorkAllocationRevisionRangeRule(int Min, int Max) : IRollingStatementOfWorkAllocationRule
{
    public List<string> Check(RollingStatementOfWorkAllocation item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling statement of work allocation.</summary>
public sealed record RollingStatementOfWorkAllocationValidator(List<IRollingStatementOfWorkAllocationRule> Rules)
{
    public List<string> Validate(RollingStatementOfWorkAllocation item)
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

    public void RequireValid(RollingStatementOfWorkAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingStatementOfWorkAllocation> items)
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

/// <summary>Functions of rolling statement of work allocation rules.</summary>
public static class RollingStatementOfWorkAllocationRules
{
    public static RollingStatementOfWorkAllocationValidator DefaultRollingStatementOfWorkAllocationValidator()
    {
        return new RollingStatementOfWorkAllocationValidator(
            new List<IRollingStatementOfWorkAllocationRule>
            {
                new RollingStatementOfWorkAllocationAllowedCodeRule(new List<string>()),
                new RollingStatementOfWorkAllocationCodePrefixRule("OPS"),
                new RollingStatementOfWorkAllocationCeilingRangeRule(6.0, 3077.1),
                new RollingStatementOfWorkAllocationRequiredCodeRule(
                    "rolling statement of work allocation needs this field"),
                new RollingStatementOfWorkAllocationDiscountRangeRule(9.6, 960.1),
                new RollingStatementOfWorkAllocationRevisionRangeRule(16, 3600),
            });
    }
}
