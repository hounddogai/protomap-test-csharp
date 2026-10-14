namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding score is in its life cycle.</summary>
public enum OffboardingScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A offboarding score as the offboarding team keeps it.</summary>
public sealed class OffboardingScore(string code, string label, double cost, double hours, int version, int updatedAt, OffboardingScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public double Hours { get; set; } = hours;
    public int Version { get; set; } = version;
    public int UpdatedAt { get; set; } = updatedAt;
    public OffboardingScoreStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding score in status requested.</summary>
    public static OffboardingScore NewOffboardingScore(string code, string label)
    {
        return new OffboardingScore(code, label, 0.0, 0.0, 0, 0, OffboardingScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingScoreStatus next)
    {
        switch (Status)
        {
            case OffboardingScoreStatus.Requested:
                return next == OffboardingScoreStatus.Granted || next == OffboardingScoreStatus.Expired;
            case OffboardingScoreStatus.Granted:
                switch (next)
                {
                    case OffboardingScoreStatus.Revoked:
                    case OffboardingScoreStatus.Expired:
                        return true;
                }
                return false;
            case OffboardingScoreStatus.Revoked:
                return next == OffboardingScoreStatus.Expired;
        }
        return false;
    }

    public void MoveTo(OffboardingScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Version + (double)UpdatedAt, 2);
    }

    public OffboardingScore Copy()
    {
        return new OffboardingScore(Code, Label, Cost, Hours, Version, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
