namespace Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>A rule a career path ledger must pass. Check returns its problems, or none.</summary>
public interface ICareerPathLedgerRule
{
    List<string> Check(CareerPathLedger item);
}

public sealed record CareerPathLedgerStartsAtRangeRule(int Min, int Max) : ICareerPathLedgerRule
{
    public List<string> Check(CareerPathLedger item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathLedgerRequiredLabelRule(string Message) : ICareerPathLedgerRule
{
    public List<string> Check(CareerPathLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathLedgerRequiredJobCodeRule(string Message) : ICareerPathLedgerRule
{
    public List<string> Check(CareerPathLedger item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathLedgerWeightRangeRule(double Min, double Max) : ICareerPathLedgerRule
{
    public List<string> Check(CareerPathLedger item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathLedgerCodePrefixRule(string Prefix) : ICareerPathLedgerRule
{
    public List<string> Check(CareerPathLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path ledger.</summary>
public sealed record CareerPathLedgerValidator(List<ICareerPathLedgerRule> Rules)
{
    public List<string> Validate(CareerPathLedger item)
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

    public void RequireValid(CareerPathLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathLedger> items)
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

/// <summary>Functions of career path ledger rules.</summary>
public static class CareerPathLedgerRules
{
    public static CareerPathLedgerValidator DefaultCareerPathLedgerValidator()
    {
        return new CareerPathLedgerValidator(
            new List<ICareerPathLedgerRule>
            {
                new CareerPathLedgerStartsAtRangeRule(16, 32),
                new CareerPathLedgerRequiredLabelRule("career path ledger needs this field"),
                new CareerPathLedgerRequiredJobCodeRule("career path ledger needs an entry"),
                new CareerPathLedgerWeightRangeRule(3.7, 4529.1),
                new CareerPathLedgerCodePrefixRule("ACM"),
            });
    }
}
