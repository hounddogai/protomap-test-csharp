namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered desk variance is in its life cycle.</summary>
public enum TieredDeskVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered desk variance as the desk team keeps it.</summary>
public sealed class TieredDeskVariance(string code, string label, string notes, double hours, double fte, int updatedAt, TieredDeskVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public double Fte { get; set; } = fte;
    public int UpdatedAt { get; set; } = updatedAt;
    public TieredDeskVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a tiered desk variance in status created.</summary>
    public static TieredDeskVariance NewTieredDeskVariance(string code, string label)
    {
        return new TieredDeskVariance(code, label, "", 0.0, 0.0, 0, TieredDeskVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredDeskVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredDeskVarianceStatus next)
    {
        switch (Status)
        {
            case TieredDeskVarianceStatus.Created:
                return next == TieredDeskVarianceStatus.Validated || next == TieredDeskVarianceStatus.Retired;
            case TieredDeskVarianceStatus.Validated:
                switch (next)
                {
                    case TieredDeskVarianceStatus.Published:
                        return true;
                }
                return false;
            case TieredDeskVarianceStatus.Published:
                switch (next)
                {
                    case TieredDeskVarianceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredDeskVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered desk variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Fte + (double)UpdatedAt, 2);
    }

    public TieredDeskVariance Copy()
    {
        return new TieredDeskVariance(Code, Label, Notes, Hours, Fte, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
