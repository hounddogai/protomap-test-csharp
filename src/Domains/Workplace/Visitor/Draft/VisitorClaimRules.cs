namespace Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>A rule a visitor claim must pass. Check returns its problems, or none.</summary>
public interface IVisitorClaimRule
{
    List<string> Check(VisitorClaim item);
}

public sealed record VisitorClaimCreatedAtRangeRule(int Min, int Max) : IVisitorClaimRule
{
    public List<string> Check(VisitorClaim item)
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

public sealed record VisitorClaimRateRangeRule(double Min, double Max) : IVisitorClaimRule
{
    public List<string> Check(VisitorClaim item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorClaimCodeLengthRule(int MaxLength) : IVisitorClaimRule
{
    public List<string> Check(VisitorClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor claim.</summary>
public sealed record VisitorClaimValidator(List<IVisitorClaimRule> Rules)
{
    public List<string> Validate(VisitorClaim item)
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

    public void RequireValid(VisitorClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorClaim> items)
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

/// <summary>Functions of visitor claim rules.</summary>
public static class VisitorClaimRules
{
    public static VisitorClaimValidator DefaultVisitorClaimValidator()
    {
        return new VisitorClaimValidator(
            new List<IVisitorClaimRule>
            {
                new VisitorClaimCreatedAtRangeRule(80, 80),
                new VisitorClaimRateRangeRule(5.4, 3451.7),
                new VisitorClaimCodeLengthRule(120),
            });
    }
}
