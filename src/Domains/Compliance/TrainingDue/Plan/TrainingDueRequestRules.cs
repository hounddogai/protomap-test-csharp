namespace Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>A rule a training due request must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueRequestRule
{
    List<string> Check(TrainingDueRequest item);
}

public sealed record TrainingDueRequestRequiredOwnerIdRule(string Message) : ITrainingDueRequestRule
{
    public List<string> Check(TrainingDueRequest item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueRequestFteRangeRule(double Min, double Max) : ITrainingDueRequestRule
{
    public List<string> Check(TrainingDueRequest item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueRequestCodePrefixRule(string Prefix) : ITrainingDueRequestRule
{
    public List<string> Check(TrainingDueRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueRequestRequiredTitleRule(string Message) : ITrainingDueRequestRule
{
    public List<string> Check(TrainingDueRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due request.</summary>
public sealed record TrainingDueRequestValidator(List<ITrainingDueRequestRule> Rules)
{
    public List<string> Validate(TrainingDueRequest item)
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

    public void RequireValid(TrainingDueRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueRequest> items)
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

/// <summary>Functions of training due request rules.</summary>
public static class TrainingDueRequestRules
{
    public static TrainingDueRequestValidator DefaultTrainingDueRequestValidator()
    {
        return new TrainingDueRequestValidator(
            new List<ITrainingDueRequestRule>
            {
                new TrainingDueRequestRequiredOwnerIdRule("training due request needs an entry"),
                new TrainingDueRequestFteRangeRule(6.8, 4432.0),
                new TrainingDueRequestCodePrefixRule("REF"),
                new TrainingDueRequestRequiredTitleRule("training due request needs a value"),
            });
    }
}
