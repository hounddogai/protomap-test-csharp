namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle budget must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleBudgetRule
{
    List<string> Check(MeritCycleBudget item);
}

public sealed record MeritCycleBudgetRequiredJobCodeRule(string Message) : IMeritCycleBudgetRule
{
    public List<string> Check(MeritCycleBudget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBudgetCodeLengthRule(int MaxLength) : IMeritCycleBudgetRule
{
    public List<string> Check(MeritCycleBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBudgetRequiredCodeRule(string Message) : IMeritCycleBudgetRule
{
    public List<string> Check(MeritCycleBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBudgetAllowedCodeRule(List<string> Allowed) : IMeritCycleBudgetRule
{
    public List<string> Check(MeritCycleBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle budget.</summary>
public sealed record MeritCycleBudgetValidator(List<IMeritCycleBudgetRule> Rules)
{
    public List<string> Validate(MeritCycleBudget item)
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

    public void RequireValid(MeritCycleBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleBudget> items)
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

/// <summary>Functions of merit cycle budget rules.</summary>
public static class MeritCycleBudgetRules
{
    public static MeritCycleBudgetValidator DefaultMeritCycleBudgetValidator()
    {
        return new MeritCycleBudgetValidator(
            new List<IMeritCycleBudgetRule>
            {
                new MeritCycleBudgetRequiredJobCodeRule("merit cycle budget needs a value"),
                new MeritCycleBudgetCodeLengthRule(1000),
                new MeritCycleBudgetRequiredCodeRule("merit cycle budget needs an entry"),
                new MeritCycleBudgetAllowedCodeRule(new List<string> { "scheduled" }),
            });
    }
}
