namespace Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>A rule a career path proposal must pass. Check returns its problems, or none.</summary>
public interface ICareerPathProposalRule
{
    List<string> Check(CareerPathProposal item);
}

public sealed record CareerPathProposalRequiredPhoneRule(string Message) : ICareerPathProposalRule
{
    public List<string> Check(CareerPathProposal item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathProposalFteRangeRule(double Min, double Max) : ICareerPathProposalRule
{
    public List<string> Check(CareerPathProposal item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathProposalRatioRangeRule(double Min, double Max) : ICareerPathProposalRule
{
    public List<string> Check(CareerPathProposal item)
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

/// <summary>Runs every rule of a career path proposal.</summary>
public sealed record CareerPathProposalValidator(List<ICareerPathProposalRule> Rules)
{
    public List<string> Validate(CareerPathProposal item)
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

    public void RequireValid(CareerPathProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathProposal> items)
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

/// <summary>Functions of career path proposal rules.</summary>
public static class CareerPathProposalRules
{
    public static CareerPathProposalValidator DefaultCareerPathProposalValidator()
    {
        return new CareerPathProposalValidator(
            new List<ICareerPathProposalRule>
            {
                new CareerPathProposalRequiredPhoneRule("career path proposal needs this field"),
                new CareerPathProposalFteRangeRule(1.3, 3821.6),
                new CareerPathProposalRatioRangeRule(2.1, 2106.0),
            });
    }
}
