namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition baseline is in its life cycle.</summary>
public enum RecognitionBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A recognition baseline as the recognition team keeps it.</summary>
public sealed class RecognitionBaseline(string code, string name, string phone, string costCenter, double fte, string ownerId, double floorValue, RecognitionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string CostCenter { get; set; } = costCenter;
    public double Fte { get; set; } = fte;
    public string OwnerId { get; set; } = ownerId;
    public double FloorValue { get; set; } = floorValue;
    public RecognitionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a recognition baseline in status created.</summary>
    public static RecognitionBaseline NewRecognitionBaseline(string code, string name)
    {
        return new RecognitionBaseline(code, name, "", "", 0.0, "", 0.0, RecognitionBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionBaselineStatus next)
    {
        switch (Status)
        {
            case RecognitionBaselineStatus.Created:
                switch (next)
                {
                    case RecognitionBaselineStatus.Retired:
                        return true;
                }
                return false;
            case RecognitionBaselineStatus.Validated:
                return next == RecognitionBaselineStatus.Retired;
            case RecognitionBaselineStatus.Published:
                return next == RecognitionBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RecognitionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(FloorValue * Fte, 2);
    }

    public RecognitionBaseline Copy()
    {
        return new RecognitionBaseline(Code, Name, Phone, CostCenter, Fte, OwnerId, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
