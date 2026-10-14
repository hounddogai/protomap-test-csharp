namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal span profile is in its life cycle.</summary>
public enum SeasonalSpanProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One seasonal span profile, with what the planners need to act on it.</summary>
public sealed class SeasonalSpanProfile(string code, string name, double maximum, double amount, int attempts, SeasonalSpanProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public SeasonalSpanProfileStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal span profile in status created.</summary>
    public static SeasonalSpanProfile NewSeasonalSpanProfile(string code, string name)
    {
        return new SeasonalSpanProfile(code, name, 0.0, 0.0, 0, SeasonalSpanProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSpanProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSpanProfileStatus next)
    {
        switch (Status)
        {
            case SeasonalSpanProfileStatus.Created:
                return next == SeasonalSpanProfileStatus.Retired;
            case SeasonalSpanProfileStatus.Validated:
                switch (next)
                {
                    case SeasonalSpanProfileStatus.Published:
                    case SeasonalSpanProfileStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalSpanProfileStatus.Published:
                return next == SeasonalSpanProfileStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SeasonalSpanProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal span profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Amount + Maximum, 2);
    }

    public SeasonalSpanProfile Copy()
    {
        return new SeasonalSpanProfile(Code, Name, Maximum, Amount, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
