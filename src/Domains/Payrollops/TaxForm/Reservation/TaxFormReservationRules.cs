namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>A rule a tax form reservation must pass. Check returns its problems, or none.</summary>
public interface ITaxFormReservationRule
{
    List<string> Check(TaxFormReservation item);
}

public sealed record TaxFormReservationCeilingRangeRule(double Min, double Max) : ITaxFormReservationRule
{
    public List<string> Check(TaxFormReservation item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReservationRequiredCategoryRule(string Message) : ITaxFormReservationRule
{
    public List<string> Check(TaxFormReservation item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReservationRegionLengthRule(int MaxLength) : ITaxFormReservationRule
{
    public List<string> Check(TaxFormReservation item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReservationCodeLengthRule(int MaxLength) : ITaxFormReservationRule
{
    public List<string> Check(TaxFormReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormReservationPinnedGuardRule(double Limit) : ITaxFormReservationRule
{
    public List<string> Check(TaxFormReservation item)
    {
        if (item.Pinned && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form reservation.</summary>
public sealed record TaxFormReservationValidator(List<ITaxFormReservationRule> Rules)
{
    public List<string> Validate(TaxFormReservation item)
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

    public void RequireValid(TaxFormReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormReservation> items)
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

/// <summary>Functions of tax form reservation rules.</summary>
public static class TaxFormReservationRules
{
    public static TaxFormReservationValidator DefaultTaxFormReservationValidator()
    {
        return new TaxFormReservationValidator(
            new List<ITaxFormReservationRule>
            {
                new TaxFormReservationCeilingRangeRule(8.3, 471.2),
                new TaxFormReservationRequiredCategoryRule("tax form reservation needs an entry"),
                new TaxFormReservationRegionLengthRule(16),
                new TaxFormReservationCodeLengthRule(16),
                new TaxFormReservationPinnedGuardRule(1.3),
            });
    }
}
