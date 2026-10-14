namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run preference is in its life cycle.</summary>
public enum PayRunPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pay run preference, with what the planners need to act on it.</summary>
public sealed class PayRunPreference(string code, string title, double rating, double budget, int endsAt, int expiresAt, string country, PayRunPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public double Budget { get; set; } = budget;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Country { get; set; } = country;
    public PayRunPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a pay run preference in status proposed.</summary>
    public static PayRunPreference NewPayRunPreference(string code, string title)
    {
        return new PayRunPreference(code, title, 0.0, 0.0, 0, 0, "", PayRunPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunPreferenceStatus next)
    {
        switch (Status)
        {
            case PayRunPreferenceStatus.Proposed:
                switch (next)
                {
                    case PayRunPreferenceStatus.Approved:
                        return true;
                }
                return false;
            case PayRunPreferenceStatus.Approved:
                switch (next)
                {
                    case PayRunPreferenceStatus.Rejected:
                    case PayRunPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunPreferenceStatus.Rejected:
                return next == PayRunPreferenceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayRunPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pay run preference"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt * (double)EndsAt, 2);
    }

    public PayRunPreference Copy()
    {
        return new PayRunPreference(Code, Title, Rating, Budget, EndsAt, ExpiresAt, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
