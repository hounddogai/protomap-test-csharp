namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary reorg plan must pass. Check returns its problems, or none.</summary>
public interface IPrimaryReorgPlanRule
{
    List<string> Check(PrimaryReorgPlan item);
}

public sealed record PrimaryReorgPlanAllowedCodeRule(List<string> Allowed) : IPrimaryReorgPlanRule
{
    public List<string> Check(PrimaryReorgPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgPlanRequiredCodeRule(string Message) : IPrimaryReorgPlanRule
{
    public List<string> Check(PrimaryReorgPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryReorgPlanLocationLengthRule(int MaxLength) : IPrimaryReorgPlanRule
{
    public List<string> Check(PrimaryReorgPlan item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary reorg plan.</summary>
public sealed record PrimaryReorgPlanValidator(List<IPrimaryReorgPlanRule> Rules)
{
    public List<string> Validate(PrimaryReorgPlan item)
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

    public void RequireValid(PrimaryReorgPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryReorgPlan> items)
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

/// <summary>Functions of primary reorg plan rules.</summary>
public static class PrimaryReorgPlanRules
{
    public static PrimaryReorgPlanValidator DefaultPrimaryReorgPlanValidator()
    {
        return new PrimaryReorgPlanValidator(
            new List<IPrimaryReorgPlanRule>
            {
                new PrimaryReorgPlanAllowedCodeRule(new List<string>()),
                new PrimaryReorgPlanRequiredCodeRule("primary reorg plan needs a value"),
                new PrimaryReorgPlanLocationLengthRule(80),
            });
    }
}
