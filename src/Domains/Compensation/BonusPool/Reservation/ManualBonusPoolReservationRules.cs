namespace Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>A rule a manual bonus pool reservation must pass. Check returns its problems, or none.</summary>
public interface IManualBonusPoolReservationRule
{
    List<string> Check(ManualBonusPoolReservation item);
}

public sealed record ManualBonusPoolReservationLabelLengthRule(int MaxLength) : IManualBonusPoolReservationRule
{
    public List<string> Check(ManualBonusPoolReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolReservationCostRangeRule(double Min, double Max) : IManualBonusPoolReservationRule
{
    public List<string> Check(ManualBonusPoolReservation item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolReservationCodeLengthRule(int MaxLength) : IManualBonusPoolReservationRule
{
    public List<string> Check(ManualBonusPoolReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolReservationCategoryLengthRule(int MaxLength) : IManualBonusPoolReservationRule
{
    public List<string> Check(ManualBonusPoolReservation item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual bonus pool reservation.</summary>
public sealed record ManualBonusPoolReservationValidator(List<IManualBonusPoolReservationRule> Rules)
{
    public List<string> Validate(ManualBonusPoolReservation item)
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

    public void RequireValid(ManualBonusPoolReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBonusPoolReservation> items)
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

/// <summary>Functions of manual bonus pool reservation rules.</summary>
public static class ManualBonusPoolReservationRules
{
    public static ManualBonusPoolReservationValidator DefaultManualBonusPoolReservationValidator()
    {
        return new ManualBonusPoolReservationValidator(
            new List<IManualBonusPoolReservationRule>
            {
                new ManualBonusPoolReservationLabelLengthRule(80),
                new ManualBonusPoolReservationCostRangeRule(0.1, 3692.5),
                new ManualBonusPoolReservationCodeLengthRule(32),
                new ManualBonusPoolReservationCategoryLengthRule(1000),
            });
    }
}
