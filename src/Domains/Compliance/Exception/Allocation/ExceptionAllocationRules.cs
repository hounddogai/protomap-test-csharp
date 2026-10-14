namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception allocation must pass. Check returns its problems, or none.</summary>
public interface IExceptionAllocationRule
{
    List<string> Check(ExceptionAllocation item);
}

public sealed record ExceptionAllocationAllowedTeamRule(List<string> Allowed) : IExceptionAllocationRule
{
    public List<string> Check(ExceptionAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is stale" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAllocationSkillLengthRule(int MaxLength) : IExceptionAllocationRule
{
    public List<string> Check(ExceptionAllocation item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAllocationNameLengthRule(int MaxLength) : IExceptionAllocationRule
{
    public List<string> Check(ExceptionAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAllocationRequiredCodeRule(string Message) : IExceptionAllocationRule
{
    public List<string> Check(ExceptionAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAllocationRatioRangeRule(double Min, double Max) : IExceptionAllocationRule
{
    public List<string> Check(ExceptionAllocation item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionAllocationRequiredNameRule(string Message) : IExceptionAllocationRule
{
    public List<string> Check(ExceptionAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception allocation.</summary>
public sealed record ExceptionAllocationValidator(List<IExceptionAllocationRule> Rules)
{
    public List<string> Validate(ExceptionAllocation item)
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

    public void RequireValid(ExceptionAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionAllocation> items)
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

/// <summary>Functions of exception allocation rules.</summary>
public static class ExceptionAllocationRules
{
    public static ExceptionAllocationValidator DefaultExceptionAllocationValidator()
    {
        return new ExceptionAllocationValidator(
            new List<IExceptionAllocationRule>
            {
                new ExceptionAllocationAllowedTeamRule(new List<string>()),
                new ExceptionAllocationSkillLengthRule(1000),
                new ExceptionAllocationNameLengthRule(120),
                new ExceptionAllocationRequiredCodeRule("exception allocation needs this field"),
                new ExceptionAllocationRatioRangeRule(4.7, 3580.1),
                new ExceptionAllocationRequiredNameRule("exception allocation needs an entry"),
            });
    }
}
