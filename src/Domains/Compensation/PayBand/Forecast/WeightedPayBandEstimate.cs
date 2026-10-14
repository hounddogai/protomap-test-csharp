namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted pay band estimate is in its life cycle.</summary>
public enum WeightedPayBandEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted pay band estimate. Its code is unique within a tenant.</summary>
public sealed class WeightedPayBandEstimate(string code, string label, double fte, int attempts, double maximum, string team, WeightedPayBandEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public double Maximum { get; set; } = maximum;
    public string Team { get; set; } = team;
    public WeightedPayBandEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a weighted pay band estimate in status proposed.</summary>
    public static WeightedPayBandEstimate NewWeightedPayBandEstimate(string code, string label)
    {
        return new WeightedPayBandEstimate(code, label, 0.0, 0, 0.0, "", WeightedPayBandEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPayBandEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPayBandEstimateStatus next)
    {
        switch (Status)
        {
            case WeightedPayBandEstimateStatus.Proposed:
                switch (next)
                {
                    case WeightedPayBandEstimateStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedPayBandEstimateStatus.Approved:
                return next == WeightedPayBandEstimateStatus.Withdrawn;
            case WeightedPayBandEstimateStatus.Rejected:
                return next == WeightedPayBandEstimateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedPayBandEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted pay band estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Attempts - Maximum, 2);
    }

    public WeightedPayBandEstimate Copy()
    {
        return new WeightedPayBandEstimate(Code, Label, Fte, Attempts, Maximum, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
