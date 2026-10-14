namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position reservation is in its life cycle.</summary>
public enum PositionReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A position reservation as the position team keeps it.</summary>
public sealed class PositionReservation(string code, string label, string costCenter, double ratio, double fte, string locale, Dictionary<string, string> attributes, double margin, string currency, string badge, string unit, PositionReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public double Ratio { get; set; } = ratio;
    public double Fte { get; set; } = fte;
    public string Locale { get; set; } = locale;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Margin { get; set; } = margin;
    public string Currency { get; set; } = currency;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public PositionReservationStatus Status { get; set; } = status;

    /// <summary>Starts a position reservation in status open.</summary>
    public static PositionReservation NewPositionReservation(string code, string label)
    {
        return new PositionReservation(
            code,
            label,
            "",
            0.0,
            0.0,
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            PositionReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionReservationStatus next)
    {
        switch (Status)
        {
            case PositionReservationStatus.Open:
                switch (next)
                {
                    case PositionReservationStatus.InReview:
                        return true;
                }
                return false;
            case PositionReservationStatus.InReview:
                return next == PositionReservationStatus.Archived;
            case PositionReservationStatus.Resolved:
                switch (next)
                {
                    case PositionReservationStatus.Reopened:
                        return true;
                }
                return false;
            case PositionReservationStatus.Reopened:
                switch (next)
                {
                    case PositionReservationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"position reservation"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveVolume()
    {
        return Math.Round(Fte - Margin, 2);
    }

    public PositionReservation Copy()
    {
        return new PositionReservation(
            Code,
            Label,
            CostCenter,
            Ratio,
            Fte,
            Locale,
            Attributes,
            Margin,
            Currency,
            Badge,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
