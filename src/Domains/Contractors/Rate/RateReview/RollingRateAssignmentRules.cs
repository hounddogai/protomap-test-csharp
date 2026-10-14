namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling rate assignment must pass. Check returns its problems, or none.</summary>
public interface IRollingRateAssignmentRule
{
    List<string> Check(RollingRateAssignment item);
}

public sealed record RollingRateAssignmentCodeLengthRule(int MaxLength) : IRollingRateAssignmentRule
{
    public List<string> Check(RollingRateAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateAssignmentAllowedSourceRule(List<string> Allowed) : IRollingRateAssignmentRule
{
    public List<string> Check(RollingRateAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateAssignmentRequiredLabelRule(string Message) : IRollingRateAssignmentRule
{
    public List<string> Check(RollingRateAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRateAssignmentApprovedGuardRule(int Limit) : IRollingRateAssignmentRule
{
    public List<string> Check(RollingRateAssignment item)
    {
        if (item.Approved && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateAssignmentSequenceRangeRule(int Min, int Max) : IRollingRateAssignmentRule
{
    public List<string> Check(RollingRateAssignment item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling rate assignment.</summary>
public sealed record RollingRateAssignmentValidator(List<IRollingRateAssignmentRule> Rules)
{
    public List<string> Validate(RollingRateAssignment item)
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

    public void RequireValid(RollingRateAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRateAssignment> items)
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

/// <summary>Functions of rolling rate assignment rules.</summary>
public static class RollingRateAssignmentRules
{
    public static RollingRateAssignmentValidator DefaultRollingRateAssignmentValidator()
    {
        return new RollingRateAssignmentValidator(
            new List<IRollingRateAssignmentRule>
            {
                new RollingRateAssignmentCodeLengthRule(255),
                new RollingRateAssignmentAllowedSourceRule(new List<string> { "pending", "capped" }),
                new RollingRateAssignmentRequiredLabelRule("rolling rate assignment needs a value"),
                new RollingRateAssignmentApprovedGuardRule(3600),
                new RollingRateAssignmentSequenceRangeRule(32, 64),
            });
    }
}
