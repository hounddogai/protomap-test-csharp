namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model reservation must pass. Check returns its problems, or none.</summary>
public interface ICostModelReservationRule
{
    List<string> Check(CostModelReservation item);
}

public sealed record CostModelReservationRequiredLabelRule(string Message) : ICostModelReservationRule
{
    public List<string> Check(CostModelReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelReservationActiveGuardRule(int Limit) : ICostModelReservationRule
{
    public List<string> Check(CostModelReservation item)
    {
        if (item.Active && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record CostModelReservationLabelLengthRule(int MaxLength) : ICostModelReservationRule
{
    public List<string> Check(CostModelReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelReservationCodeLengthRule(int MaxLength) : ICostModelReservationRule
{
    public List<string> Check(CostModelReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelReservationRequiredCodeRule(string Message) : ICostModelReservationRule
{
    public List<string> Check(CostModelReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelReservationAllowedLabelRule(List<string> Allowed) : ICostModelReservationRule
{
    public List<string> Check(CostModelReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model reservation.</summary>
public sealed record CostModelReservationValidator(List<ICostModelReservationRule> Rules)
{
    public List<string> Validate(CostModelReservation item)
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

    public void RequireValid(CostModelReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelReservation> items)
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

/// <summary>Functions of cost model reservation rules.</summary>
public static class CostModelReservationRules
{
    public static CostModelReservationValidator DefaultCostModelReservationValidator()
    {
        return new CostModelReservationValidator(
            new List<ICostModelReservationRule>
            {
                new CostModelReservationRequiredLabelRule("cost model reservation needs a value"),
                new CostModelReservationActiveGuardRule(86400),
                new CostModelReservationLabelLengthRule(80),
                new CostModelReservationCodeLengthRule(16),
                new CostModelReservationRequiredCodeRule("cost model reservation needs this field"),
                new CostModelReservationAllowedLabelRule(new List<string> { "adaptive" }),
            });
    }
}
