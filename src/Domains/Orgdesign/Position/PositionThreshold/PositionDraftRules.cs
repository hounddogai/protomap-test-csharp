namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position draft must pass. Check returns its problems, or none.</summary>
public interface IPositionDraftRule
{
    List<string> Check(PositionDraft item);
}

public sealed record PositionDraftCodeLengthRule(int MaxLength) : IPositionDraftRule
{
    public List<string> Check(PositionDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionDraftNameLengthRule(int MaxLength) : IPositionDraftRule
{
    public List<string> Check(PositionDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionDraftScoreRangeRule(double Min, double Max) : IPositionDraftRule
{
    public List<string> Check(PositionDraft item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionDraftAllowedLocationRule(List<string> Allowed) : IPositionDraftRule
{
    public List<string> Check(PositionDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position draft.</summary>
public sealed record PositionDraftValidator(List<IPositionDraftRule> Rules)
{
    public List<string> Validate(PositionDraft item)
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

    public void RequireValid(PositionDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionDraft> items)
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

/// <summary>Functions of position draft rules.</summary>
public static class PositionDraftRules
{
    public static PositionDraftValidator DefaultPositionDraftValidator()
    {
        return new PositionDraftValidator(
            new List<IPositionDraftRule>
            {
                new PositionDraftCodeLengthRule(1000),
                new PositionDraftNameLengthRule(80),
                new PositionDraftScoreRangeRule(5.8, 1635.6),
                new PositionDraftAllowedLocationRule(new List<string>()),
            });
    }
}
