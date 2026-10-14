namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered assignment agreement must pass. Check returns its problems, or none.</summary>
public interface ITieredAssignmentAgreementRule
{
    List<string> Check(TieredAssignmentAgreement item);
}

public sealed record TieredAssignmentAgreementRequiredLabelRule(string Message) : ITieredAssignmentAgreementRule
{
    public List<string> Check(TieredAssignmentAgreement item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredAssignmentAgreementCreatedAtRangeRule(int Min, int Max) : ITieredAssignmentAgreementRule
{
    public List<string> Check(TieredAssignmentAgreement item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredAssignmentAgreementAllowedLabelRule(List<string> Allowed) : ITieredAssignmentAgreementRule
{
    public List<string> Check(TieredAssignmentAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered assignment agreement.</summary>
public sealed record TieredAssignmentAgreementValidator(List<ITieredAssignmentAgreementRule> Rules)
{
    public List<string> Validate(TieredAssignmentAgreement item)
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

    public void RequireValid(TieredAssignmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAssignmentAgreement> items)
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

/// <summary>Functions of tiered assignment agreement rules.</summary>
public static class TieredAssignmentAgreementRules
{
    public static TieredAssignmentAgreementValidator DefaultTieredAssignmentAgreementValidator()
    {
        return new TieredAssignmentAgreementValidator(
            new List<ITieredAssignmentAgreementRule>
            {
                new TieredAssignmentAgreementRequiredLabelRule("tiered assignment agreement needs an entry"),
                new TieredAssignmentAgreementCreatedAtRangeRule(80, 86400),
                new TieredAssignmentAgreementAllowedLabelRule(new List<string> { "default" }),
            });
    }
}
