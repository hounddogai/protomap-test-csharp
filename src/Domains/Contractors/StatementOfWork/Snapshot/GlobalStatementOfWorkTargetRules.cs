namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>A rule a global statement of work target must pass. Check returns its problems, or none.</summary>
public interface IGlobalStatementOfWorkTargetRule
{
    List<string> Check(GlobalStatementOfWorkTarget item);
}

public sealed record GlobalStatementOfWorkTargetRequiredNameRule(string Message) : IGlobalStatementOfWorkTargetRule
{
    public List<string> Check(GlobalStatementOfWorkTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalStatementOfWorkTargetCodePrefixRule(string Prefix) : IGlobalStatementOfWorkTargetRule
{
    public List<string> Check(GlobalStatementOfWorkTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalStatementOfWorkTargetRequiredJobCodeRule(string Message) : IGlobalStatementOfWorkTargetRule
{
    public List<string> Check(GlobalStatementOfWorkTarget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global statement of work target.</summary>
public sealed record GlobalStatementOfWorkTargetValidator(List<IGlobalStatementOfWorkTargetRule> Rules)
{
    public List<string> Validate(GlobalStatementOfWorkTarget item)
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

    public void RequireValid(GlobalStatementOfWorkTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalStatementOfWorkTarget> items)
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

/// <summary>Functions of global statement of work target rules.</summary>
public static class GlobalStatementOfWorkTargetRules
{
    public static GlobalStatementOfWorkTargetValidator DefaultGlobalStatementOfWorkTargetValidator()
    {
        return new GlobalStatementOfWorkTargetValidator(
            new List<IGlobalStatementOfWorkTargetRule>
            {
                new GlobalStatementOfWorkTargetRequiredNameRule("global statement of work target needs a value"),
                new GlobalStatementOfWorkTargetCodePrefixRule("OPS"),
                new GlobalStatementOfWorkTargetRequiredJobCodeRule(
                    "global statement of work target needs this field"),
            });
    }
}
