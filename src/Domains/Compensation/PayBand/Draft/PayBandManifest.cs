namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band manifest is in its life cycle.</summary>
public enum PayBandManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pay band manifest as the pay band team keeps it.</summary>
public sealed class PayBandManifest(string code, string title, double ceiling, string phone, int sequence, string category, PayBandManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public string Category { get; set; } = category;
    public PayBandManifestStatus Status { get; set; } = status;

    /// <summary>Starts a pay band manifest in status open.</summary>
    public static PayBandManifest NewPayBandManifest(string code, string title)
    {
        return new PayBandManifest(code, title, 0.0, "", 0, "", PayBandManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandManifestStatus next)
    {
        switch (Status)
        {
            case PayBandManifestStatus.Open:
                return next == PayBandManifestStatus.Archived;
            case PayBandManifestStatus.InReview:
                switch (next)
                {
                    case PayBandManifestStatus.Resolved:
                        return true;
                }
                return false;
            case PayBandManifestStatus.Resolved:
                return next == PayBandManifestStatus.Archived;
            case PayBandManifestStatus.Reopened:
                switch (next)
                {
                    case PayBandManifestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band manifest"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling - (double)Sequence, 2);
    }

    public PayBandManifest Copy()
    {
        return new PayBandManifest(Code, Title, Ceiling, Phone, Sequence, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
