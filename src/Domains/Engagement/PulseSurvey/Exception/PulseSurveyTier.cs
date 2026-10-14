namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey tier is in its life cycle.</summary>
public enum PulseSurveyTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pulse survey tier as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyTier(string code, string name, double floorValue, int expiresAt, string notes, double rate, PulseSurveyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public double Rate { get; set; } = rate;
    public PulseSurveyTierStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey tier in status requested.</summary>
    public static PulseSurveyTier NewPulseSurveyTier(string code, string name)
    {
        return new PulseSurveyTier(code, name, 0.0, 0, "", 0.0, PulseSurveyTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyTierStatus next)
    {
        switch (Status)
        {
            case PulseSurveyTierStatus.Requested:
                return next == PulseSurveyTierStatus.Granted || next == PulseSurveyTierStatus.Expired;
            case PulseSurveyTierStatus.Granted:
                switch (next)
                {
                    case PulseSurveyTierStatus.Expired:
                        return true;
                }
                return false;
            case PulseSurveyTierStatus.Revoked:
                switch (next)
                {
                    case PulseSurveyTierStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate * (double)ExpiresAt, 2);
    }

    public PulseSurveyTier Copy()
    {
        return new PulseSurveyTier(Code, Name, FloorValue, ExpiresAt, Notes, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
