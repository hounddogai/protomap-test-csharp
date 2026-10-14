namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted pay band preference is in its life cycle.</summary>
public enum WeightedPayBandPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One weighted pay band preference, with what the planners need to act on it.</summary>
public sealed class WeightedPayBandPreference(string code, string title, bool archived, string reviewer, int retries, double amount, string name, double minimum, string locale, string costCenter, WeightedPayBandPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Locale { get; set; } = locale;
    public string CostCenter { get; set; } = costCenter;
    public WeightedPayBandPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted pay band preference in status created.</summary>
    public static WeightedPayBandPreference NewWeightedPayBandPreference(string code, string title)
    {
        return new WeightedPayBandPreference(
            code,
            title,
            false,
            "",
            0,
            0.0,
            "",
            0.0,
            "",
            "",
            WeightedPayBandPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPayBandPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPayBandPreferenceStatus next)
    {
        switch (Status)
        {
            case WeightedPayBandPreferenceStatus.Created:
                switch (next)
                {
                    case WeightedPayBandPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case WeightedPayBandPreferenceStatus.Validated:
                return next == WeightedPayBandPreferenceStatus.Published || next == WeightedPayBandPreferenceStatus.Retired;
            case WeightedPayBandPreferenceStatus.Published:
                return next == WeightedPayBandPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedPayBandPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted pay band preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted pay band preference"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Retries + Minimum, 2);
    }

    public WeightedPayBandPreference Copy()
    {
        return new WeightedPayBandPreference(
            Code,
            Title,
            Archived,
            Reviewer,
            Retries,
            Amount,
            Name,
            Minimum,
            Locale,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
