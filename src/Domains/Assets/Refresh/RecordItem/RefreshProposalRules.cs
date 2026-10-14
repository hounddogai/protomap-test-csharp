namespace Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>A rule a refresh proposal must pass. Check returns its problems, or none.</summary>
public interface IRefreshProposalRule
{
    List<string> Check(RefreshProposal item);
}

public sealed record RefreshProposalCodePrefixRule(string Prefix) : IRefreshProposalRule
{
    public List<string> Check(RefreshProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshProposalJobCodeLengthRule(int MaxLength) : IRefreshProposalRule
{
    public List<string> Check(RefreshProposal item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshProposalWindowDaysRangeRule(int Min, int Max) : IRefreshProposalRule
{
    public List<string> Check(RefreshProposal item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a refresh proposal.</summary>
public sealed record RefreshProposalValidator(List<IRefreshProposalRule> Rules)
{
    public List<string> Validate(RefreshProposal item)
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

    public void RequireValid(RefreshProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshProposal> items)
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

/// <summary>Functions of refresh proposal rules.</summary>
public static class RefreshProposalRules
{
    public static RefreshProposalValidator DefaultRefreshProposalValidator()
    {
        return new RefreshProposalValidator(
            new List<IRefreshProposalRule>
            {
                new RefreshProposalCodePrefixRule("Q"),
                new RefreshProposalJobCodeLengthRule(3600),
                new RefreshProposalWindowDaysRangeRule(64, 3600),
            });
    }
}
