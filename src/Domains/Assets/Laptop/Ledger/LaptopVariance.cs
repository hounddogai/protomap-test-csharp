namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop variance is in its life cycle.</summary>
public enum LaptopVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a laptop variance and its current status.</summary>
public sealed class LaptopVariance(string code, string title, string costCenter, string phone, int expiresAt, LaptopVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string Phone { get; set; } = phone;
    public int ExpiresAt { get; set; } = expiresAt;
    public LaptopVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a laptop variance in status created.</summary>
    public static LaptopVariance NewLaptopVariance(string code, string title)
    {
        return new LaptopVariance(code, title, "", "", 0, LaptopVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopVarianceStatus next)
    {
        switch (Status)
        {
            case LaptopVarianceStatus.Created:
                switch (next)
                {
                    case LaptopVarianceStatus.Validated:
                    case LaptopVarianceStatus.Published:
                        return true;
                }
                return false;
            case LaptopVarianceStatus.Validated:
                return next == LaptopVarianceStatus.Published || next == LaptopVarianceStatus.Retired;
            case LaptopVarianceStatus.Published:
                return next == LaptopVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LaptopVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public LaptopVariance Copy()
    {
        return new LaptopVariance(Code, Title, CostCenter, Phone, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
