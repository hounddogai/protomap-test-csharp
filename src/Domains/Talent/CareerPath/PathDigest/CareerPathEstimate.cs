namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path estimate is in its life cycle.</summary>
public enum CareerPathEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A career path estimate. Its code is unique within a tenant.</summary>
public sealed class CareerPathEstimate(string code, string name, string unit, double minimum, int attempts, CareerPathEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Minimum { get; set; } = minimum;
    public int Attempts { get; set; } = attempts;
    public CareerPathEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a career path estimate in status requested.</summary>
    public static CareerPathEstimate NewCareerPathEstimate(string code, string name)
    {
        return new CareerPathEstimate(code, name, "", 0.0, 0, CareerPathEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathEstimateStatus next)
    {
        switch (Status)
        {
            case CareerPathEstimateStatus.Requested:
                switch (next)
                {
                    case CareerPathEstimateStatus.Granted:
                    case CareerPathEstimateStatus.Revoked:
                        return true;
                }
                return false;
            case CareerPathEstimateStatus.Granted:
                switch (next)
                {
                    case CareerPathEstimateStatus.Revoked:
                    case CareerPathEstimateStatus.Expired:
                        return true;
                }
                return false;
            case CareerPathEstimateStatus.Revoked:
                return next == CareerPathEstimateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CareerPathEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum - (double)Attempts, 2);
    }

    public CareerPathEstimate Copy()
    {
        return new CareerPathEstimate(Code, Name, Unit, Minimum, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
