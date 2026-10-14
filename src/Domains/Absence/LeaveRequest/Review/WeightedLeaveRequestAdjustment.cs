namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted leave request adjustment is in its life cycle.</summary>
public enum WeightedLeaveRequestAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted leave request adjustment. Its code is unique within a tenant.</summary>
public sealed class WeightedLeaveRequestAdjustment(string code, string label, string title, string description, string checksum, double hours, double score, string source, string phone, int quantity, int retries, WeightedLeaveRequestAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Checksum { get; set; } = checksum;
    public double Hours { get; set; } = hours;
    public double Score { get; set; } = score;
    public string Source { get; set; } = source;
    public string Phone { get; set; } = phone;
    public int Quantity { get; set; } = quantity;
    public int Retries { get; set; } = retries;
    public WeightedLeaveRequestAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a weighted leave request adjustment in status proposed.</summary>
    public static WeightedLeaveRequestAdjustment NewWeightedLeaveRequestAdjustment(string code, string label)
    {
        return new WeightedLeaveRequestAdjustment(
            code,
            label,
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            0,
            WeightedLeaveRequestAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLeaveRequestAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLeaveRequestAdjustmentStatus next)
    {
        switch (Status)
        {
            case WeightedLeaveRequestAdjustmentStatus.Proposed:
                switch (next)
                {
                    case WeightedLeaveRequestAdjustmentStatus.Approved:
                    case WeightedLeaveRequestAdjustmentStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedLeaveRequestAdjustmentStatus.Approved:
                switch (next)
                {
                    case WeightedLeaveRequestAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedLeaveRequestAdjustmentStatus.Rejected:
                switch (next)
                {
                    case WeightedLeaveRequestAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedLeaveRequestAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted leave request adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted leave request adjustment"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Quantity + (double)Retries, 2);
    }

    public WeightedLeaveRequestAdjustment Copy()
    {
        return new WeightedLeaveRequestAdjustment(
            Code,
            Label,
            Title,
            Description,
            Checksum,
            Hours,
            Score,
            Source,
            Phone,
            Quantity,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
