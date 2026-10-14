namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted balance threshold is in its life cycle.</summary>
public enum WeightedBalanceThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A weighted balance threshold as the balance team keeps it.</summary>
public sealed class WeightedBalanceThreshold(string code, string label, string source, double margin, string region, string timezone, WeightedBalanceThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public string Timezone { get; set; } = timezone;
    public WeightedBalanceThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a weighted balance threshold in status draft.</summary>
    public static WeightedBalanceThreshold NewWeightedBalanceThreshold(string code, string label)
    {
        return new WeightedBalanceThreshold(code, label, "", 0.0, "", "", WeightedBalanceThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBalanceThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBalanceThresholdStatus next)
    {
        switch (Status)
        {
            case WeightedBalanceThresholdStatus.Draft:
                switch (next)
                {
                    case WeightedBalanceThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedBalanceThresholdStatus.Pending:
                switch (next)
                {
                    case WeightedBalanceThresholdStatus.Active:
                    case WeightedBalanceThresholdStatus.Closed:
                        return true;
                }
                return false;
            case WeightedBalanceThresholdStatus.Active:
                return next == WeightedBalanceThresholdStatus.Closed;
            case WeightedBalanceThresholdStatus.Suspended:
                switch (next)
                {
                    case WeightedBalanceThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedBalanceThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted balance threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted balance threshold"} {Code}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public WeightedBalanceThreshold Copy()
    {
        return new WeightedBalanceThreshold(Code, Label, Source, Margin, Region, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
