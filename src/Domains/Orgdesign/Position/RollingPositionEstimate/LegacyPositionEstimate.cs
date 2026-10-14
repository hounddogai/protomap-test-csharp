namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position estimate is in its life cycle.</summary>
public enum LegacyPositionEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A legacy position estimate. Its code is unique within a tenant.</summary>
public sealed class LegacyPositionEstimate(string code, string label, double budget, string currency, double discount, double rate, double margin, LegacyPositionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Currency { get; set; } = currency;
    public double Discount { get; set; } = discount;
    public double Rate { get; set; } = rate;
    public double Margin { get; set; } = margin;
    public LegacyPositionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position estimate in status created.</summary>
    public static LegacyPositionEstimate NewLegacyPositionEstimate(string code, string label)
    {
        return new LegacyPositionEstimate(code, label, 0.0, "", 0.0, 0.0, 0.0, LegacyPositionEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionEstimateStatus next)
    {
        switch (Status)
        {
            case LegacyPositionEstimateStatus.Created:
                switch (next)
                {
                    case LegacyPositionEstimateStatus.Published:
                        return true;
                }
                return false;
            case LegacyPositionEstimateStatus.Validated:
                return next == LegacyPositionEstimateStatus.Published || next == LegacyPositionEstimateStatus.Retired;
            case LegacyPositionEstimateStatus.Published:
                switch (next)
                {
                    case LegacyPositionEstimateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPositionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Discount + Margin, 2);
    }

    public LegacyPositionEstimate Copy()
    {
        return new LegacyPositionEstimate(Code, Label, Budget, Currency, Discount, Rate, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
