namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty score is in its life cycle.</summary>
public enum WarrantyScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a warranty score and its current status.</summary>
public sealed class WarrantyScore(string code, string label, int quantity, string name, string department, double score, string segment, string grade, double minimum, WarrantyScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public WarrantyScoreStatus Status { get; set; } = status;

    /// <summary>Starts a warranty score in status created.</summary>
    public static WarrantyScore NewWarrantyScore(string code, string label)
    {
        return new WarrantyScore(code, label, 0, "", "", 0.0, "", "", 0.0, WarrantyScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyScoreStatus next)
    {
        switch (Status)
        {
            case WarrantyScoreStatus.Created:
                return next == WarrantyScoreStatus.Validated || next == WarrantyScoreStatus.Published;
            case WarrantyScoreStatus.Validated:
                switch (next)
                {
                    case WarrantyScoreStatus.Retired:
                        return true;
                }
                return false;
            case WarrantyScoreStatus.Published:
                return next == WarrantyScoreStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WarrantyScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round(Minimum * (double)Quantity, 2);
    }

    public WarrantyScore Copy()
    {
        return new WarrantyScore(Code, Label, Quantity, Name, Department, Score, Segment, Grade, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
