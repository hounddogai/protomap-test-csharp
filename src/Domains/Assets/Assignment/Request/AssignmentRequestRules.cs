namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment request must pass. Check returns its problems, or none.</summary>
public interface IAssignmentRequestRule
{
    List<string> Check(AssignmentRequest item);
}

public sealed record AssignmentRequestAllowedTimezoneRule(List<string> Allowed) : IAssignmentRequestRule
{
    public List<string> Check(AssignmentRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentRequestScoreRangeRule(double Min, double Max) : IAssignmentRequestRule
{
    public List<string> Check(AssignmentRequest item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentRequestArchivedGuardRule(double Limit) : IAssignmentRequestRule
{
    public List<string> Check(AssignmentRequest item)
    {
        if (item.Archived && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentRequestRequiredCodeRule(string Message) : IAssignmentRequestRule
{
    public List<string> Check(AssignmentRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment request.</summary>
public sealed record AssignmentRequestValidator(List<IAssignmentRequestRule> Rules)
{
    public List<string> Validate(AssignmentRequest item)
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

    public void RequireValid(AssignmentRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentRequest> items)
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

/// <summary>Functions of assignment request rules.</summary>
public static class AssignmentRequestRules
{
    public static AssignmentRequestValidator DefaultAssignmentRequestValidator()
    {
        return new AssignmentRequestValidator(
            new List<IAssignmentRequestRule>
            {
                new AssignmentRequestAllowedTimezoneRule(new List<string>()),
                new AssignmentRequestScoreRangeRule(2.4, 4884.0),
                new AssignmentRequestArchivedGuardRule(0.8),
                new AssignmentRequestRequiredCodeRule("assignment request needs this field"),
            });
    }
}
