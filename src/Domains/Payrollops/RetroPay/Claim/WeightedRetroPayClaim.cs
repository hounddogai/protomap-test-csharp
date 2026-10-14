namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted retro pay claim is in its life cycle.</summary>
public enum WeightedRetroPayClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted retro pay claim as the retro pay team keeps it.</summary>
public sealed class WeightedRetroPayClaim(string code, string title, double minimum, string managerId, string locale, string currency, string contactEmail, int capacity, WeightedRetroPayClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public string Locale { get; set; } = locale;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public int Capacity { get; set; } = capacity;
    public WeightedRetroPayClaimStatus Status { get; set; } = status;

    /// <summary>Starts a weighted retro pay claim in status created.</summary>
    public static WeightedRetroPayClaim NewWeightedRetroPayClaim(string code, string title)
    {
        return new WeightedRetroPayClaim(code, title, 0.0, "", "", "", "", 0, WeightedRetroPayClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRetroPayClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRetroPayClaimStatus next)
    {
        switch (Status)
        {
            case WeightedRetroPayClaimStatus.Created:
                switch (next)
                {
                    case WeightedRetroPayClaimStatus.Published:
                        return true;
                }
                return false;
            case WeightedRetroPayClaimStatus.Validated:
                switch (next)
                {
                    case WeightedRetroPayClaimStatus.Published:
                    case WeightedRetroPayClaimStatus.Retired:
                        return true;
                }
                return false;
            case WeightedRetroPayClaimStatus.Published:
                return next == WeightedRetroPayClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedRetroPayClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted retro pay claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Capacity + Minimum, 2);
    }

    public WeightedRetroPayClaim Copy()
    {
        return new WeightedRetroPayClaim(
            Code,
            Title,
            Minimum,
            ManagerId,
            Locale,
            Currency,
            ContactEmail,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
