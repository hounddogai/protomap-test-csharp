namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>A rule a cost model record must pass. Check returns its problems, or none.</summary>
public interface ICostModelRecordRule
{
    List<string> Check(CostModelRecord item);
}

public sealed record CostModelRecordRequiredLocationRule(string Message) : ICostModelRecordRule
{
    public List<string> Check(CostModelRecord item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelRecordRevisionRangeRule(int Min, int Max) : ICostModelRecordRule
{
    public List<string> Check(CostModelRecord item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelRecordCodeLengthRule(int MaxLength) : ICostModelRecordRule
{
    public List<string> Check(CostModelRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model record.</summary>
public sealed record CostModelRecordValidator(List<ICostModelRecordRule> Rules)
{
    public List<string> Validate(CostModelRecord item)
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

    public void RequireValid(CostModelRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelRecord> items)
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

/// <summary>Functions of cost model record rules.</summary>
public static class CostModelRecordRules
{
    public static CostModelRecordValidator DefaultCostModelRecordValidator()
    {
        return new CostModelRecordValidator(
            new List<ICostModelRecordRule>
            {
                new CostModelRecordRequiredLocationRule("cost model record needs a value"),
                new CostModelRecordRevisionRangeRule(120, 86400),
                new CostModelRecordCodeLengthRule(1000),
            });
    }
}
