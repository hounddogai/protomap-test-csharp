namespace Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>A rule a return plan ledger must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanLedgerRule
{
    List<string> Check(ReturnPlanLedger item);
}

public sealed record ReturnPlanLedgerCeilingRangeRule(double Min, double Max) : IReturnPlanLedgerRule
{
    public List<string> Check(ReturnPlanLedger item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanLedgerRequiredCodeRule(string Message) : IReturnPlanLedgerRule
{
    public List<string> Check(ReturnPlanLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanLedgerCountryLengthRule(int MaxLength) : IReturnPlanLedgerRule
{
    public List<string> Check(ReturnPlanLedger item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanLedgerRequiredUnitRule(string Message) : IReturnPlanLedgerRule
{
    public List<string> Check(ReturnPlanLedger item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanLedgerRequiredCountryRule(string Message) : IReturnPlanLedgerRule
{
    public List<string> Check(ReturnPlanLedger item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan ledger.</summary>
public sealed record ReturnPlanLedgerValidator(List<IReturnPlanLedgerRule> Rules)
{
    public List<string> Validate(ReturnPlanLedger item)
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

    public void RequireValid(ReturnPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanLedger> items)
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

/// <summary>Functions of return plan ledger rules.</summary>
public static class ReturnPlanLedgerRules
{
    public static ReturnPlanLedgerValidator DefaultReturnPlanLedgerValidator()
    {
        return new ReturnPlanLedgerValidator(
            new List<IReturnPlanLedgerRule>
            {
                new ReturnPlanLedgerCeilingRangeRule(4.4, 1904.1),
                new ReturnPlanLedgerRequiredCodeRule("return plan ledger needs an entry"),
                new ReturnPlanLedgerCountryLengthRule(120),
                new ReturnPlanLedgerRequiredUnitRule("return plan ledger needs an entry"),
                new ReturnPlanLedgerRequiredCountryRule("return plan ledger needs a value"),
            });
    }
}
