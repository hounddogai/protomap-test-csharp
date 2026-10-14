namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>A rule a tiered training due audit must pass. Check returns its problems, or none.</summary>
public interface ITieredTrainingDueAuditRule
{
    List<string> Check(TieredTrainingDueAudit item);
}

public sealed record TieredTrainingDueAuditContactEmailLengthRule(int MaxLength) : ITieredTrainingDueAuditRule
{
    public List<string> Check(TieredTrainingDueAudit item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueAuditRequiredNameRule(string Message) : ITieredTrainingDueAuditRule
{
    public List<string> Check(TieredTrainingDueAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueAuditRequiredChannelRule(string Message) : ITieredTrainingDueAuditRule
{
    public List<string> Check(TieredTrainingDueAudit item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueAuditChannelLengthRule(int MaxLength) : ITieredTrainingDueAuditRule
{
    public List<string> Check(TieredTrainingDueAudit item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTrainingDueAuditCodePrefixRule(string Prefix) : ITieredTrainingDueAuditRule
{
    public List<string> Check(TieredTrainingDueAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered training due audit.</summary>
public sealed record TieredTrainingDueAuditValidator(List<ITieredTrainingDueAuditRule> Rules)
{
    public List<string> Validate(TieredTrainingDueAudit item)
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

    public void RequireValid(TieredTrainingDueAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredTrainingDueAudit> items)
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

/// <summary>Functions of tiered training due audit rules.</summary>
public static class TieredTrainingDueAuditRules
{
    public static TieredTrainingDueAuditValidator DefaultTieredTrainingDueAuditValidator()
    {
        return new TieredTrainingDueAuditValidator(
            new List<ITieredTrainingDueAuditRule>
            {
                new TieredTrainingDueAuditContactEmailLengthRule(32),
                new TieredTrainingDueAuditRequiredNameRule("tiered training due audit needs a value"),
                new TieredTrainingDueAuditRequiredChannelRule("tiered training due audit needs a value"),
                new TieredTrainingDueAuditChannelLengthRule(120),
                new TieredTrainingDueAuditCodePrefixRule("Q"),
            });
    }
}
