namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment estimate is in its life cycle.</summary>
public enum AdjustmentEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adjustment estimate and its current status.</summary>
public sealed class AdjustmentEstimate(string code, string title, double rate, double hours, bool archived, string department, double margin, string locale, string label, bool active, double cost, AdjustmentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public double Hours { get; set; } = hours;
    public bool Archived { get; set; } = archived;
    public string Department { get; set; } = department;
    public double Margin { get; set; } = margin;
    public string Locale { get; set; } = locale;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public AdjustmentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment estimate in status requested.</summary>
    public static AdjustmentEstimate NewAdjustmentEstimate(string code, string title)
    {
        return new AdjustmentEstimate(
            code,
            title,
            0.0,
            0.0,
            false,
            "",
            0.0,
            "",
            "",
            false,
            0.0,
            AdjustmentEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentEstimateStatus next)
    {
        switch (Status)
        {
            case AdjustmentEstimateStatus.Requested:
                switch (next)
                {
                    case AdjustmentEstimateStatus.Granted:
                    case AdjustmentEstimateStatus.Expired:
                        return true;
                }
                return false;
            case AdjustmentEstimateStatus.Granted:
                switch (next)
                {
                    case AdjustmentEstimateStatus.Expired:
                        return true;
                }
                return false;
            case AdjustmentEstimateStatus.Revoked:
                switch (next)
                {
                    case AdjustmentEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Margin * Rate, 2);
    }

    public AdjustmentEstimate Copy()
    {
        return new AdjustmentEstimate(
            Code,
            Title,
            Rate,
            Hours,
            Archived,
            Department,
            Margin,
            Locale,
            Label,
            Active,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
