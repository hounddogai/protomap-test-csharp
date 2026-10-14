namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>A rule a training due proposal must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueProposalRule
{
    List<string> Check(TrainingDueProposal item);
}

public sealed record TrainingDueProposalCodePrefixRule(string Prefix) : ITrainingDueProposalRule
{
    public List<string> Check(TrainingDueProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueProposalMinimumRangeRule(double Min, double Max) : ITrainingDueProposalRule
{
    public List<string> Check(TrainingDueProposal item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueProposalLimitRangeRule(int Min, int Max) : ITrainingDueProposalRule
{
    public List<string> Check(TrainingDueProposal item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a training due proposal.</summary>
public sealed record TrainingDueProposalValidator(List<ITrainingDueProposalRule> Rules)
{
    public List<string> Validate(TrainingDueProposal item)
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

    public void RequireValid(TrainingDueProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueProposal> items)
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

/// <summary>Functions of training due proposal rules.</summary>
public static class TrainingDueProposalRules
{
    public static TrainingDueProposalValidator DefaultTrainingDueProposalValidator()
    {
        return new TrainingDueProposalValidator(
            new List<ITrainingDueProposalRule>
            {
                new TrainingDueProposalCodePrefixRule("REF"),
                new TrainingDueProposalMinimumRangeRule(4.1, 132.7),
                new TrainingDueProposalLimitRangeRule(120, 255),
            });
    }
}
