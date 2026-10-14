namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>A rule a capped tax form assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedTaxFormAssignmentRule
{
    List<string> Check(CappedTaxFormAssignment item);
}

public sealed record CappedTaxFormAssignmentSequenceRangeRule(int Min, int Max) : ICappedTaxFormAssignmentRule
{
    public List<string> Check(CappedTaxFormAssignment item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedTaxFormAssignmentExpiresAtRangeRule(int Min, int Max) : ICappedTaxFormAssignmentRule
{
    public List<string> Check(CappedTaxFormAssignment item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTaxFormAssignmentRequiredCodeRule(string Message) : ICappedTaxFormAssignmentRule
{
    public List<string> Check(CappedTaxFormAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedTaxFormAssignmentRequiredSourceRule(string Message) : ICappedTaxFormAssignmentRule
{
    public List<string> Check(CappedTaxFormAssignment item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedTaxFormAssignmentSourceLengthRule(int MaxLength) : ICappedTaxFormAssignmentRule
{
    public List<string> Check(CappedTaxFormAssignment item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped tax form assignment.</summary>
public sealed record CappedTaxFormAssignmentValidator(List<ICappedTaxFormAssignmentRule> Rules)
{
    public List<string> Validate(CappedTaxFormAssignment item)
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

    public void RequireValid(CappedTaxFormAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTaxFormAssignment> items)
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

/// <summary>Functions of capped tax form assignment rules.</summary>
public static class CappedTaxFormAssignmentRules
{
    public static CappedTaxFormAssignmentValidator DefaultCappedTaxFormAssignmentValidator()
    {
        return new CappedTaxFormAssignmentValidator(
            new List<ICappedTaxFormAssignmentRule>
            {
                new CappedTaxFormAssignmentSequenceRangeRule(16, 80),
                new CappedTaxFormAssignmentExpiresAtRangeRule(1000, 3600),
                new CappedTaxFormAssignmentRequiredCodeRule("capped tax form assignment needs this field"),
                new CappedTaxFormAssignmentRequiredSourceRule("capped tax form assignment needs a value"),
                new CappedTaxFormAssignmentSourceLengthRule(3600),
            });
    }
}
