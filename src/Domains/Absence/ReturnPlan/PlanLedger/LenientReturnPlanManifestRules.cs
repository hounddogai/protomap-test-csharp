namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

/// <summary>A rule a lenient return plan manifest must pass. Check returns its problems, or none.</summary>
public interface ILenientReturnPlanManifestRule
{
    List<string> Check(LenientReturnPlanManifest item);
}

public sealed record LenientReturnPlanManifestCodePrefixRule(string Prefix) : ILenientReturnPlanManifestRule
{
    public List<string> Check(LenientReturnPlanManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanManifestAmountRangeRule(double Min, double Max) : ILenientReturnPlanManifestRule
{
    public List<string> Check(LenientReturnPlanManifest item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientReturnPlanManifestCodeLengthRule(int MaxLength) : ILenientReturnPlanManifestRule
{
    public List<string> Check(LenientReturnPlanManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanManifestTenureMonthsRangeRule(int Min, int Max) : ILenientReturnPlanManifestRule
{
    public List<string> Check(LenientReturnPlanManifest item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanManifestRequiredLabelRule(string Message) : ILenientReturnPlanManifestRule
{
    public List<string> Check(LenientReturnPlanManifest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanManifestDurationMinutesRangeRule(int Min, int Max) : ILenientReturnPlanManifestRule
{
    public List<string> Check(LenientReturnPlanManifest item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient return plan manifest.</summary>
public sealed record LenientReturnPlanManifestValidator(List<ILenientReturnPlanManifestRule> Rules)
{
    public List<string> Validate(LenientReturnPlanManifest item)
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

    public void RequireValid(LenientReturnPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientReturnPlanManifest> items)
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

/// <summary>Functions of lenient return plan manifest rules.</summary>
public static class LenientReturnPlanManifestRules
{
    public static LenientReturnPlanManifestValidator DefaultLenientReturnPlanManifestValidator()
    {
        return new LenientReturnPlanManifestValidator(
            new List<ILenientReturnPlanManifestRule>
            {
                new LenientReturnPlanManifestCodePrefixRule("OPS"),
                new LenientReturnPlanManifestAmountRangeRule(3.7, 2798.1),
                new LenientReturnPlanManifestCodeLengthRule(32),
                new LenientReturnPlanManifestTenureMonthsRangeRule(120, 86400),
                new LenientReturnPlanManifestRequiredLabelRule("lenient return plan manifest needs this field"),
                new LenientReturnPlanManifestDurationMinutesRangeRule(16, 80),
            });
    }
}
