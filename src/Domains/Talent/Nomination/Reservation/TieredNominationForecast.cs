namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered nomination forecast is in its life cycle.</summary>
public enum TieredNominationForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered nomination forecast. Its code is unique within a tenant.</summary>
public sealed class TieredNominationForecast(string code, string name, Dictionary<string, string> attributes, int retries, string jobCode, double amount, TieredNominationForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public double Amount { get; set; } = amount;
    public TieredNominationForecastStatus Status { get; set; } = status;

    /// <summary>Starts a tiered nomination forecast in status idle.</summary>
    public static TieredNominationForecast NewTieredNominationForecast(string code, string name)
    {
        return new TieredNominationForecast(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            TieredNominationForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredNominationForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredNominationForecastStatus next)
    {
        switch (Status)
        {
            case TieredNominationForecastStatus.Idle:
                switch (next)
                {
                    case TieredNominationForecastStatus.Ready:
                    case TieredNominationForecastStatus.Draining:
                        return true;
                }
                return false;
            case TieredNominationForecastStatus.Warming:
                switch (next)
                {
                    case TieredNominationForecastStatus.Stopped:
                        return true;
                }
                return false;
            case TieredNominationForecastStatus.Ready:
                return next == TieredNominationForecastStatus.Draining || next == TieredNominationForecastStatus.Stopped;
            case TieredNominationForecastStatus.Draining:
                return next == TieredNominationForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredNominationForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered nomination forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered nomination forecast"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveTotal()
    {
        return Math.Round(Amount + (double)Retries, 2);
    }

    public TieredNominationForecast Copy()
    {
        return new TieredNominationForecast(Code, Name, Attributes, Retries, JobCode, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
