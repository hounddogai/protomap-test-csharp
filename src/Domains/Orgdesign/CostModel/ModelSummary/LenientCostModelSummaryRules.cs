namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient cost model summary must pass. Check returns its problems, or none.</summary>
public interface ILenientCostModelSummaryRule
{
    List<string> Check(LenientCostModelSummary item);
}

public sealed record LenientCostModelSummaryAllowedCostCenterRule(List<string> Allowed) : ILenientCostModelSummaryRule
{
    public List<string> Check(LenientCostModelSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LenientCostModelSummaryRequiredCodeRule(string Message) : ILenientCostModelSummaryRule
{
    public List<string> Check(LenientCostModelSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCostModelSummaryBadgeLengthRule(int MaxLength) : ILenientCostModelSummaryRule
{
    public List<string> Check(LenientCostModelSummary item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCostModelSummaryRequiredCostCenterRule(string Message) : ILenientCostModelSummaryRule
{
    public List<string> Check(LenientCostModelSummary item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCostModelSummaryHoursRangeRule(double Min, double Max) : ILenientCostModelSummaryRule
{
    public List<string> Check(LenientCostModelSummary item)
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

public sealed record LenientCostModelSummaryArchivedGuardRule(double Limit) : ILenientCostModelSummaryRule
{
    public List<string> Check(LenientCostModelSummary item)
    {
        if (item.Archived && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient cost model summary.</summary>
public sealed record LenientCostModelSummaryValidator(List<ILenientCostModelSummaryRule> Rules)
{
    public List<string> Validate(LenientCostModelSummary item)
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

    public void RequireValid(LenientCostModelSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientCostModelSummary> items)
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

/// <summary>Functions of lenient cost model summary rules.</summary>
public static class LenientCostModelSummaryRules
{
    public static LenientCostModelSummaryValidator DefaultLenientCostModelSummaryValidator()
    {
        return new LenientCostModelSummaryValidator(
            new List<ILenientCostModelSummaryRule>
            {
                new LenientCostModelSummaryAllowedCostCenterRule(new List<string>()),
                new LenientCostModelSummaryRequiredCodeRule("lenient cost model summary needs an entry"),
                new LenientCostModelSummaryBadgeLengthRule(64),
                new LenientCostModelSummaryRequiredCostCenterRule("lenient cost model summary needs an entry"),
                new LenientCostModelSummaryHoursRangeRule(3.8, 2067.5),
                new LenientCostModelSummaryArchivedGuardRule(7.4),
            });
    }
}
