namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form adjustment is in its life cycle.</summary>
public enum TaxFormAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tax form adjustment as the tax form team keeps it.</summary>
public sealed class TaxFormAdjustment(string code, string label, string skill, double rating, int startsAt, double minimum, double margin, TaxFormAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public int StartsAt { get; set; } = startsAt;
    public double Minimum { get; set; } = minimum;
    public double Margin { get; set; } = margin;
    public TaxFormAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a tax form adjustment in status open.</summary>
    public static TaxFormAdjustment NewTaxFormAdjustment(string code, string label)
    {
        return new TaxFormAdjustment(code, label, "", 0.0, 0, 0.0, 0.0, TaxFormAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormAdjustmentStatus next)
    {
        switch (Status)
        {
            case TaxFormAdjustmentStatus.Open:
                switch (next)
                {
                    case TaxFormAdjustmentStatus.Reopened:
                        return true;
                }
                return false;
            case TaxFormAdjustmentStatus.InReview:
                switch (next)
                {
                    case TaxFormAdjustmentStatus.Resolved:
                    case TaxFormAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case TaxFormAdjustmentStatus.Resolved:
                switch (next)
                {
                    case TaxFormAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case TaxFormAdjustmentStatus.Reopened:
                switch (next)
                {
                    case TaxFormAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Margin - Rating, 2);
    }

    public TaxFormAdjustment Copy()
    {
        return new TaxFormAdjustment(Code, Label, Skill, Rating, StartsAt, Minimum, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
