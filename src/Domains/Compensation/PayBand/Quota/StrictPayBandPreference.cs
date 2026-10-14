namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pay band preference is in its life cycle.</summary>
public enum StrictPayBandPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a strict pay band preference and its current status.</summary>
public sealed class StrictPayBandPreference(string code, string name, string description, int limit, int sequence, string grade, StrictPayBandPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public int Sequence { get; set; } = sequence;
    public string Grade { get; set; } = grade;
    public StrictPayBandPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a strict pay band preference in status created.</summary>
    public static StrictPayBandPreference NewStrictPayBandPreference(string code, string name)
    {
        return new StrictPayBandPreference(code, name, "", 0, 0, "", StrictPayBandPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPayBandPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPayBandPreferenceStatus next)
    {
        switch (Status)
        {
            case StrictPayBandPreferenceStatus.Created:
                return next == StrictPayBandPreferenceStatus.Validated || next == StrictPayBandPreferenceStatus.Published;
            case StrictPayBandPreferenceStatus.Validated:
                switch (next)
                {
                    case StrictPayBandPreferenceStatus.Published:
                        return true;
                }
                return false;
            case StrictPayBandPreferenceStatus.Published:
                return next == StrictPayBandPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StrictPayBandPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pay band preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Limit - (double)Sequence, 2);
    }

    public StrictPayBandPreference Copy()
    {
        return new StrictPayBandPreference(Code, Name, Description, Limit, Sequence, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
