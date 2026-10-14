namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band score is in its life cycle.</summary>
public enum PayBandScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pay band score, with what the planners need to act on it.</summary>
public sealed class PayBandScore(string code, string label, int revision, string category, string title, string unit, int quantity, string name, double minimum, bool approved, PayBandScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Category { get; set; } = category;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public int Quantity { get; set; } = quantity;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public bool Approved { get; set; } = approved;
    public PayBandScoreStatus Status { get; set; } = status;

    /// <summary>Starts a pay band score in status draft.</summary>
    public static PayBandScore NewPayBandScore(string code, string label)
    {
        return new PayBandScore(code, label, 0, "", "", "", 0, "", 0.0, false, PayBandScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandScoreStatus next)
    {
        switch (Status)
        {
            case PayBandScoreStatus.Draft:
                return next == PayBandScoreStatus.Active;
            case PayBandScoreStatus.Pending:
                switch (next)
                {
                    case PayBandScoreStatus.Suspended:
                    case PayBandScoreStatus.Closed:
                        return true;
                }
                return false;
            case PayBandScoreStatus.Active:
                return next == PayBandScoreStatus.Closed;
            case PayBandScoreStatus.Suspended:
                return next == PayBandScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayBandScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Quantity * Minimum, 2);
    }

    public PayBandScore Copy()
    {
        return new PayBandScore(
            Code,
            Label,
            Revision,
            Category,
            Title,
            Unit,
            Quantity,
            Name,
            Minimum,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
