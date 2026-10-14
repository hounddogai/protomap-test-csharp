namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy span window is in its life cycle.</summary>
public enum LegacySpanWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A legacy span window. Its code is unique within a tenant.</summary>
public sealed class LegacySpanWindow(string code, string label, double discount, string notes, double weight, LegacySpanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public double Weight { get; set; } = weight;
    public LegacySpanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a legacy span window in status created.</summary>
    public static LegacySpanWindow NewLegacySpanWindow(string code, string label)
    {
        return new LegacySpanWindow(code, label, 0.0, "", 0.0, LegacySpanWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySpanWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySpanWindowStatus next)
    {
        switch (Status)
        {
            case LegacySpanWindowStatus.Created:
                return next == LegacySpanWindowStatus.Published || next == LegacySpanWindowStatus.Retired;
            case LegacySpanWindowStatus.Validated:
                switch (next)
                {
                    case LegacySpanWindowStatus.Published:
                    case LegacySpanWindowStatus.Retired:
                        return true;
                }
                return false;
            case LegacySpanWindowStatus.Published:
                switch (next)
                {
                    case LegacySpanWindowStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacySpanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy span window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy span window"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight + Discount, 2);
    }

    public LegacySpanWindow Copy()
    {
        return new LegacySpanWindow(Code, Label, Discount, Notes, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
