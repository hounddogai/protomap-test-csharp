namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy rate preference is in its life cycle.</summary>
public enum LegacyRatePreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy rate preference, with what the planners need to act on it.</summary>
public sealed class LegacyRatePreference(string code, string title, string jobCode, bool active, int quantity, int priority, int batchSize, string grade, double ceiling, LegacyRatePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public bool Active { get; set; } = active;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public int BatchSize { get; set; } = batchSize;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public LegacyRatePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy rate preference in status created.</summary>
    public static LegacyRatePreference NewLegacyRatePreference(string code, string title)
    {
        return new LegacyRatePreference(
            code,
            title,
            "",
            false,
            0,
            0,
            0,
            "",
            0.0,
            LegacyRatePreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRatePreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRatePreferenceStatus next)
    {
        switch (Status)
        {
            case LegacyRatePreferenceStatus.Created:
                return next == LegacyRatePreferenceStatus.Validated || next == LegacyRatePreferenceStatus.Published;
            case LegacyRatePreferenceStatus.Validated:
                switch (next)
                {
                    case LegacyRatePreferenceStatus.Retired:
                        return true;
                }
                return false;
            case LegacyRatePreferenceStatus.Published:
                switch (next)
                {
                    case LegacyRatePreferenceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyRatePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy rate preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy rate preference"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)BatchSize - Ceiling, 2);
    }

    public LegacyRatePreference Copy()
    {
        return new LegacyRatePreference(
            Code,
            Title,
            JobCode,
            Active,
            Quantity,
            Priority,
            BatchSize,
            Grade,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
