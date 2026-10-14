namespace Acme.Workforce.Talent.CareerPath.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path adjustment is in its life cycle.</summary>
public enum CareerPathAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a career path adjustment and its current status.</summary>
public sealed class CareerPathAdjustment(string code, string title, string costCenter, string managerId, double discount, string label, string ownerId, string channel, CareerPathAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Channel { get; set; } = channel;
    public CareerPathAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a career path adjustment in status open.</summary>
    public static CareerPathAdjustment NewCareerPathAdjustment(string code, string title)
    {
        return new CareerPathAdjustment(code, title, "", "", 0.0, "", "", "", CareerPathAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathAdjustmentStatus next)
    {
        switch (Status)
        {
            case CareerPathAdjustmentStatus.Open:
                return next == CareerPathAdjustmentStatus.Reopened;
            case CareerPathAdjustmentStatus.InReview:
                switch (next)
                {
                    case CareerPathAdjustmentStatus.Resolved:
                        return true;
                }
                return false;
            case CareerPathAdjustmentStatus.Resolved:
                switch (next)
                {
                    case CareerPathAdjustmentStatus.Reopened:
                        return true;
                }
                return false;
            case CareerPathAdjustmentStatus.Reopened:
                switch (next)
                {
                    case CareerPathAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public CareerPathAdjustment Copy()
    {
        return new CareerPathAdjustment(
            Code,
            Title,
            CostCenter,
            ManagerId,
            Discount,
            Label,
            OwnerId,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
