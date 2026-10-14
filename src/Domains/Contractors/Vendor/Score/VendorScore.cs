namespace Acme.Workforce.Contractors.Vendor.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor score is in its life cycle.</summary>
public enum VendorScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a vendor score and its current status.</summary>
public sealed class VendorScore(string code, string label, int updatedAt, double hours, double budget, int attempts, VendorScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Hours { get; set; } = hours;
    public double Budget { get; set; } = budget;
    public int Attempts { get; set; } = attempts;
    public VendorScoreStatus Status { get; set; } = status;

    /// <summary>Starts a vendor score in status draft.</summary>
    public static VendorScore NewVendorScore(string code, string label)
    {
        return new VendorScore(code, label, 0, 0.0, 0.0, 0, VendorScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorScoreStatus next)
    {
        switch (Status)
        {
            case VendorScoreStatus.Draft:
                return next == VendorScoreStatus.Suspended;
            case VendorScoreStatus.Pending:
                return next == VendorScoreStatus.Active || next == VendorScoreStatus.Closed;
            case VendorScoreStatus.Active:
                return next == VendorScoreStatus.Suspended || next == VendorScoreStatus.Closed;
            case VendorScoreStatus.Suspended:
                switch (next)
                {
                    case VendorScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)UpdatedAt + (double)Attempts, 2);
    }

    public VendorScore Copy()
    {
        return new VendorScore(Code, Label, UpdatedAt, Hours, Budget, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
