namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty claim is in its life cycle.</summary>
public enum WarrantyClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A warranty claim. Its code is unique within a tenant.</summary>
public sealed class WarrantyClaim(string code, string name, int attempts, double rating, int revision, string timezone, double fte, string externalRef, WarrantyClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public double Rating { get; set; } = rating;
    public int Revision { get; set; } = revision;
    public string Timezone { get; set; } = timezone;
    public double Fte { get; set; } = fte;
    public string ExternalRef { get; set; } = externalRef;
    public WarrantyClaimStatus Status { get; set; } = status;

    /// <summary>Starts a warranty claim in status open.</summary>
    public static WarrantyClaim NewWarrantyClaim(string code, string name)
    {
        return new WarrantyClaim(code, name, 0, 0.0, 0, "", 0.0, "", WarrantyClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyClaimStatus next)
    {
        switch (Status)
        {
            case WarrantyClaimStatus.Open:
                switch (next)
                {
                    case WarrantyClaimStatus.Resolved:
                    case WarrantyClaimStatus.Reopened:
                        return true;
                }
                return false;
            case WarrantyClaimStatus.InReview:
                switch (next)
                {
                    case WarrantyClaimStatus.Archived:
                        return true;
                }
                return false;
            case WarrantyClaimStatus.Resolved:
                switch (next)
                {
                    case WarrantyClaimStatus.Reopened:
                    case WarrantyClaimStatus.Archived:
                        return true;
                }
                return false;
            case WarrantyClaimStatus.Reopened:
                switch (next)
                {
                    case WarrantyClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Fte - (double)Revision, 2);
    }

    public WarrantyClaim Copy()
    {
        return new WarrantyClaim(Code, Name, Attempts, Rating, Revision, Timezone, Fte, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
