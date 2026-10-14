namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy agreement must pass. Check returns its problems, or none.</summary>
public interface IOccupancyAgreementRule
{
    List<string> Check(OccupancyAgreement item);
}

public sealed record OccupancyAgreementTeamLengthRule(int MaxLength) : IOccupancyAgreementRule
{
    public List<string> Check(OccupancyAgreement item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAgreementRequiredTeamRule(string Message) : IOccupancyAgreementRule
{
    public List<string> Check(OccupancyAgreement item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAgreementCodeLengthRule(int MaxLength) : IOccupancyAgreementRule
{
    public List<string> Check(OccupancyAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAgreementAllowedTeamRule(List<string> Allowed) : IOccupancyAgreementRule
{
    public List<string> Check(OccupancyAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAgreementTitleLengthRule(int MaxLength) : IOccupancyAgreementRule
{
    public List<string> Check(OccupancyAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAgreementRequiredCostCenterRule(string Message) : IOccupancyAgreementRule
{
    public List<string> Check(OccupancyAgreement item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy agreement.</summary>
public sealed record OccupancyAgreementValidator(List<IOccupancyAgreementRule> Rules)
{
    public List<string> Validate(OccupancyAgreement item)
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

    public void RequireValid(OccupancyAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyAgreement> items)
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

/// <summary>Functions of occupancy agreement rules.</summary>
public static class OccupancyAgreementRules
{
    public static OccupancyAgreementValidator DefaultOccupancyAgreementValidator()
    {
        return new OccupancyAgreementValidator(
            new List<IOccupancyAgreementRule>
            {
                new OccupancyAgreementTeamLengthRule(86400),
                new OccupancyAgreementRequiredTeamRule("occupancy agreement needs a value"),
                new OccupancyAgreementCodeLengthRule(1000),
                new OccupancyAgreementAllowedTeamRule(new List<string> { "scheduled", "lenient" }),
                new OccupancyAgreementTitleLengthRule(32),
                new OccupancyAgreementRequiredCostCenterRule("occupancy agreement needs an entry"),
            });
    }
}
