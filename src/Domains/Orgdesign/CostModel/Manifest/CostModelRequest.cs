namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model request is in its life cycle.</summary>
public enum CostModelRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A cost model request as the cost model team keeps it.</summary>
public sealed class CostModelRequest(string code, string label, double discount, string currency, int startsAt, int retries, CostModelRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Currency { get; set; } = currency;
    public int StartsAt { get; set; } = startsAt;
    public int Retries { get; set; } = retries;
    public CostModelRequestStatus Status { get; set; } = status;

    /// <summary>Starts a cost model request in status open.</summary>
    public static CostModelRequest NewCostModelRequest(string code, string label)
    {
        return new CostModelRequest(code, label, 0.0, "", 0, 0, CostModelRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelRequestStatus next)
    {
        switch (Status)
        {
            case CostModelRequestStatus.Open:
                switch (next)
                {
                    case CostModelRequestStatus.InReview:
                    case CostModelRequestStatus.Archived:
                        return true;
                }
                return false;
            case CostModelRequestStatus.InReview:
                switch (next)
                {
                    case CostModelRequestStatus.Reopened:
                        return true;
                }
                return false;
            case CostModelRequestStatus.Resolved:
                return next == CostModelRequestStatus.Reopened || next == CostModelRequestStatus.Archived;
            case CostModelRequestStatus.Reopened:
                switch (next)
                {
                    case CostModelRequestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Retries - (double)StartsAt, 2);
    }

    public CostModelRequest Copy()
    {
        return new CostModelRequest(Code, Label, Discount, Currency, StartsAt, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
