namespace Acme.Workforce.Talent.Mobility.Variance;

/// <summary>A rule a primary mobility proposal must pass. Check returns its problems, or none.</summary>
public interface IPrimaryMobilityProposalRule
{
    List<string> Check(PrimaryMobilityProposal item);
}

public sealed record PrimaryMobilityProposalTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryMobilityProposalRule
{
    public List<string> Check(PrimaryMobilityProposal item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record PrimaryMobilityProposalRequiredGradeRule(string Message) : IPrimaryMobilityProposalRule
{
    public List<string> Check(PrimaryMobilityProposal item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMobilityProposalGradeLengthRule(int MaxLength) : IPrimaryMobilityProposalRule
{
    public List<string> Check(PrimaryMobilityProposal item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary mobility proposal.</summary>
public sealed record PrimaryMobilityProposalValidator(List<IPrimaryMobilityProposalRule> Rules)
{
    public List<string> Validate(PrimaryMobilityProposal item)
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

    public void RequireValid(PrimaryMobilityProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryMobilityProposal> items)
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

/// <summary>Functions of primary mobility proposal rules.</summary>
public static class PrimaryMobilityProposalRules
{
    public static PrimaryMobilityProposalValidator DefaultPrimaryMobilityProposalValidator()
    {
        return new PrimaryMobilityProposalValidator(
            new List<IPrimaryMobilityProposalRule>
            {
                new PrimaryMobilityProposalTagsLimitRule(32, new List<string> { "batched" }),
                new PrimaryMobilityProposalRequiredGradeRule("primary mobility proposal needs a value"),
                new PrimaryMobilityProposalGradeLengthRule(80),
            });
    }
}
