namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return record is in its life cycle.</summary>
public enum AssetReturnRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a asset return record and its current status.</summary>
public sealed class AssetReturnRecord(string code, string label, string country, double rate, double hours, string grade, string managerId, AssetReturnRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public double Rate { get; set; } = rate;
    public double Hours { get; set; } = hours;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public AssetReturnRecordStatus Status { get; set; } = status;

    /// <summary>Starts a asset return record in status draft.</summary>
    public static AssetReturnRecord NewAssetReturnRecord(string code, string label)
    {
        return new AssetReturnRecord(code, label, "", 0.0, 0.0, "", "", AssetReturnRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnRecordStatus next)
    {
        switch (Status)
        {
            case AssetReturnRecordStatus.Draft:
                switch (next)
                {
                    case AssetReturnRecordStatus.Pending:
                    case AssetReturnRecordStatus.Suspended:
                        return true;
                }
                return false;
            case AssetReturnRecordStatus.Pending:
                switch (next)
                {
                    case AssetReturnRecordStatus.Active:
                        return true;
                }
                return false;
            case AssetReturnRecordStatus.Active:
                return next == AssetReturnRecordStatus.Suspended;
            case AssetReturnRecordStatus.Suspended:
                return next == AssetReturnRecordStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AssetReturnRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Rate + Hours, 2);
    }

    public AssetReturnRecord Copy()
    {
        return new AssetReturnRecord(Code, Label, Country, Rate, Hours, Grade, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
