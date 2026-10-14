namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment variance is in its life cycle.</summary>
public enum GarnishmentVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A garnishment variance. Its code is unique within a tenant.</summary>
public sealed class GarnishmentVariance(string code, string label, int updatedAt, int retries, double cost, int tenureMonths, GarnishmentVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public int TenureMonths { get; set; } = tenureMonths;
    public GarnishmentVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment variance in status created.</summary>
    public static GarnishmentVariance NewGarnishmentVariance(string code, string label)
    {
        return new GarnishmentVariance(code, label, 0, 0, 0.0, 0, GarnishmentVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentVarianceStatus next)
    {
        switch (Status)
        {
            case GarnishmentVarianceStatus.Created:
                return next == GarnishmentVarianceStatus.Validated || next == GarnishmentVarianceStatus.Retired;
            case GarnishmentVarianceStatus.Validated:
                return next == GarnishmentVarianceStatus.Published || next == GarnishmentVarianceStatus.Retired;
            case GarnishmentVarianceStatus.Published:
                switch (next)
                {
                    case GarnishmentVarianceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)TenureMonths + (double)UpdatedAt, 2);
    }

    public GarnishmentVariance Copy()
    {
        return new GarnishmentVariance(Code, Label, UpdatedAt, Retries, Cost, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
