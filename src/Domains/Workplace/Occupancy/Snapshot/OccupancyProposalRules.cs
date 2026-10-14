namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>A rule a occupancy proposal must pass. Check returns its problems, or none.</summary>
public interface IOccupancyProposalRule
{
    List<string> Check(OccupancyProposal item);
}

public sealed record OccupancyProposalArchivedGuardRule(int Limit) : IOccupancyProposalRule
{
    public List<string> Check(OccupancyProposal item)
    {
        if (item.Archived && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyProposalCodeLengthRule(int MaxLength) : IOccupancyProposalRule
{
    public List<string> Check(OccupancyProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyProposalRequiredCodeRule(string Message) : IOccupancyProposalRule
{
    public List<string> Check(OccupancyProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy proposal.</summary>
public sealed record OccupancyProposalValidator(List<IOccupancyProposalRule> Rules)
{
    public List<string> Validate(OccupancyProposal item)
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

    public void RequireValid(OccupancyProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyProposal> items)
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

/// <summary>Functions of occupancy proposal rules.</summary>
public static class OccupancyProposalRules
{
    public static OccupancyProposalValidator DefaultOccupancyProposalValidator()
    {
        return new OccupancyProposalValidator(
            new List<IOccupancyProposalRule>
            {
                new OccupancyProposalArchivedGuardRule(16),
                new OccupancyProposalCodeLengthRule(16),
                new OccupancyProposalRequiredCodeRule("occupancy proposal needs a value"),
            });
    }
}
