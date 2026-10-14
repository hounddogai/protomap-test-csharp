namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted laptop audit must pass. Check returns its problems, or none.</summary>
public interface IWeightedLaptopAuditRule
{
    List<string> Check(WeightedLaptopAudit item);
}

public sealed record WeightedLaptopAuditExpiresAtRangeRule(int Min, int Max) : IWeightedLaptopAuditRule
{
    public List<string> Check(WeightedLaptopAudit item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedLaptopAuditNotesLengthRule(int MaxLength) : IWeightedLaptopAuditRule
{
    public List<string> Check(WeightedLaptopAudit item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopAuditRequiredCodeRule(string Message) : IWeightedLaptopAuditRule
{
    public List<string> Check(WeightedLaptopAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopAuditHoursRangeRule(double Min, double Max) : IWeightedLaptopAuditRule
{
    public List<string> Check(WeightedLaptopAudit item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedLaptopAuditRequiredNotesRule(string Message) : IWeightedLaptopAuditRule
{
    public List<string> Check(WeightedLaptopAudit item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopAuditAllowedReviewerRule(List<string> Allowed) : IWeightedLaptopAuditRule
{
    public List<string> Check(WeightedLaptopAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted laptop audit.</summary>
public sealed record WeightedLaptopAuditValidator(List<IWeightedLaptopAuditRule> Rules)
{
    public List<string> Validate(WeightedLaptopAudit item)
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

    public void RequireValid(WeightedLaptopAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedLaptopAudit> items)
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

/// <summary>Functions of weighted laptop audit rules.</summary>
public static class WeightedLaptopAuditRules
{
    public static WeightedLaptopAuditValidator DefaultWeightedLaptopAuditValidator()
    {
        return new WeightedLaptopAuditValidator(
            new List<IWeightedLaptopAuditRule>
            {
                new WeightedLaptopAuditExpiresAtRangeRule(80, 86400),
                new WeightedLaptopAuditNotesLengthRule(32),
                new WeightedLaptopAuditRequiredCodeRule("weighted laptop audit needs an entry"),
                new WeightedLaptopAuditHoursRangeRule(3.5, 2422.5),
                new WeightedLaptopAuditRequiredNotesRule("weighted laptop audit needs an entry"),
                new WeightedLaptopAuditAllowedReviewerRule(new List<string> { "seasonal" }),
            });
    }
}
