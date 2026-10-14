namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path claim is in its life cycle.</summary>
public enum CareerPathClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a career path claim and its current status.</summary>
public sealed class CareerPathClaim(string code, string name, string unit, double discount, int durationMinutes, int quantity, string locale, List<string> tags, string region, CareerPathClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Discount { get; set; } = discount;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Quantity { get; set; } = quantity;
    public string Locale { get; set; } = locale;
    public List<string> Tags { get; set; } = tags;
    public string Region { get; set; } = region;
    public CareerPathClaimStatus Status { get; set; } = status;

    /// <summary>Starts a career path claim in status new.</summary>
    public static CareerPathClaim NewCareerPathClaim(string code, string name)
    {
        return new CareerPathClaim(code, name, "", 0.0, 0, 0, "", new List<string>(), "", CareerPathClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathClaimStatus next)
    {
        switch (Status)
        {
            case CareerPathClaimStatus.New:
                switch (next)
                {
                    case CareerPathClaimStatus.Queued:
                    case CareerPathClaimStatus.Running:
                        return true;
                }
                return false;
            case CareerPathClaimStatus.Queued:
                return next == CareerPathClaimStatus.Failed;
            case CareerPathClaimStatus.Running:
                return next == CareerPathClaimStatus.Done || next == CareerPathClaimStatus.Failed;
            case CareerPathClaimStatus.Done:
                return next == CareerPathClaimStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CareerPathClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"career path claim"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)DurationMinutes + (double)Quantity, 2);
    }

    public CareerPathClaim Copy()
    {
        return new CareerPathClaim(
            Code,
            Name,
            Unit,
            Discount,
            DurationMinutes,
            Quantity,
            Locale,
            Tags,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
