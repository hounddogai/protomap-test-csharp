namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan window is in its life cycle.</summary>
public enum ReturnPlanWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A return plan window. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanWindow(string code, string label, double rate, string costCenter, string ownerId, int quantity, int version, bool archived, int updatedAt, double fte, string country, ReturnPlanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string CostCenter { get; set; } = costCenter;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Fte { get; set; } = fte;
    public string Country { get; set; } = country;
    public ReturnPlanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a return plan window in status open.</summary>
    public static ReturnPlanWindow NewReturnPlanWindow(string code, string label)
    {
        return new ReturnPlanWindow(code, label, 0.0, "", "", 0, 0, false, 0, 0.0, "", ReturnPlanWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanWindowStatus next)
    {
        switch (Status)
        {
            case ReturnPlanWindowStatus.Open:
                return next == ReturnPlanWindowStatus.InReview;
            case ReturnPlanWindowStatus.InReview:
                return next == ReturnPlanWindowStatus.Reopened || next == ReturnPlanWindowStatus.Archived;
            case ReturnPlanWindowStatus.Resolved:
                switch (next)
                {
                    case ReturnPlanWindowStatus.Reopened:
                    case ReturnPlanWindowStatus.Archived:
                        return true;
                }
                return false;
            case ReturnPlanWindowStatus.Reopened:
                return next == ReturnPlanWindowStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ReturnPlanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)UpdatedAt - Rate, 2);
    }

    public ReturnPlanWindow Copy()
    {
        return new ReturnPlanWindow(
            Code,
            Label,
            Rate,
            CostCenter,
            OwnerId,
            Quantity,
            Version,
            Archived,
            UpdatedAt,
            Fte,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
