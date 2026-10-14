namespace Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>A rule a asset return reservation must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnReservationRule
{
    List<string> Check(AssetReturnReservation item);
}

public sealed record AssetReturnReservationArchivedGuardRule(int Limit) : IAssetReturnReservationRule
{
    public List<string> Check(AssetReturnReservation item)
    {
        if (item.Archived && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnReservationFteRangeRule(double Min, double Max) : IAssetReturnReservationRule
{
    public List<string> Check(AssetReturnReservation item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnReservationDepartmentLengthRule(int MaxLength) : IAssetReturnReservationRule
{
    public List<string> Check(AssetReturnReservation item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return reservation.</summary>
public sealed record AssetReturnReservationValidator(List<IAssetReturnReservationRule> Rules)
{
    public List<string> Validate(AssetReturnReservation item)
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

    public void RequireValid(AssetReturnReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnReservation> items)
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

/// <summary>Functions of asset return reservation rules.</summary>
public static class AssetReturnReservationRules
{
    public static AssetReturnReservationValidator DefaultAssetReturnReservationValidator()
    {
        return new AssetReturnReservationValidator(
            new List<IAssetReturnReservationRule>
            {
                new AssetReturnReservationArchivedGuardRule(3600),
                new AssetReturnReservationFteRangeRule(3.2, 3216.2),
                new AssetReturnReservationDepartmentLengthRule(16),
            });
    }
}
